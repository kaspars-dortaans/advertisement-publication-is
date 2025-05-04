import { AuthHeaderName, TokenStorageKey } from '@/constants/auth'
import { axiosInstance } from '@/init/axios'
import { getClient } from '@/utils/client-builder'
import { ref } from 'vue'
import { RefreshRequest, UserClient, UserInfo, type LoginDto } from './api-client'
import type { ITokenInfo } from '@/types/auth/token-info'
import { Permissions } from '@/constants/api/Permissions'

export class AuthService {
  /** Singleton instance */
  private static _instance: AuthService

  /** Current user permissions */
  public static readonly permissionsPromise = ref<Promise<string[]>>(Promise.resolve([]))
  public static readonly permissions = ref<string[]>([])

  /** Basic user info */
  public static readonly profileInfoPromise = ref<Promise<UserInfo | null>>(Promise.resolve(null))
  public static readonly profileInfo = ref<UserInfo | null>(null)

  /** Is user authenticated flag */
  public static readonly isAuthenticated = ref<boolean>(false)

  /** Api user service */
  private readonly userService = getClient(UserClient)

  /** Authentication token information */
  tokenInfo: ITokenInfo | undefined

  /** Auth token refresh timeout id */
  private readonly refreshTokenTimeoutId = ref<number | null>(null)

  /** Private constructor */
  private constructor() {
    this._loadStorage()
  }

  /** Singleton getter */
  static get() {
    if (!this._instance) {
      this._instance = new AuthService()
    }

    return this._instance
  }

  /** Load stored authentication data in locale storage */
  private async _loadStorage() {
    const tokenInfo = this.parseTokenInfo(localStorage.getItem(TokenStorageKey))

    if (tokenInfo) {
      this._updateToken(tokenInfo)
      const secondsElapsed = Math.floor((Date.now() - tokenInfo.assignDate) / 1000)
      if (secondsElapsed < tokenInfo.expiresInSeconds) {
        AuthService.isAuthenticated.value = true
        this._setRefreshTimeout(tokenInfo.expiresInSeconds - secondsElapsed)
      } else {
        await this._refreshToken()
      }

      if (AuthService.isAuthenticated.value) {
        await this.refreshProfileData()
      }
    } else {
      this.logout()
    }
  }

  /** Tries to parse string to ITokenInfo */
  private parseTokenInfo(str: string | undefined | null) {
    if (!str) {
      return undefined
    }

    const object = JSON.parse(str)
    if (
      !object?.accessToken ||
      !object.refreshToken ||
      !object.expiresInSeconds ||
      !object.assignDate
    ) {
      return undefined
    }

    return {
      accessToken: object.accessToken,
      refreshToken: object.refreshToken,
      expiresInSeconds: object.expiresInSeconds,
      assignDate: parseInt(object.assignDate)
    } as ITokenInfo
  }

  /** Update stored authentication token */
  private _updateToken(tokenInfo: ITokenInfo | undefined) {
    this.tokenInfo = tokenInfo
    this._updateStorage()
    this._updateAuthorizationHeader()
  }

  /** Sync stored authentication data in locale storage */
  private _updateStorage() {
    if (this.tokenInfo) {
      localStorage.setItem(TokenStorageKey, JSON.stringify(this.tokenInfo))
    } else {
      localStorage.removeItem(TokenStorageKey)
    }
  }

  /** Update authorization header value for axios requests */
  private _updateAuthorizationHeader() {
    axiosInstance.defaults.headers.common[AuthHeaderName] = this.tokenInfo?.accessToken
      ? 'Bearer ' + this.tokenInfo.accessToken
      : undefined
  }

  /** Get profile data from Api */
  public async refreshProfileData() {
    try {
      AuthService.permissionsPromise.value = this.userService.getCurrentUserPermissions()
      AuthService.profileInfoPromise.value = this.userService.getUserInfo()
      AuthService.permissions.value = await AuthService.permissionsPromise.value
      AuthService.profileInfo.value = await AuthService.profileInfoPromise.value
      AuthService.isAuthenticated.value = true
    } catch {
      this.logout()
    }
  }

  /** Attempt to login, via sending login request to Api */
  async login(loginDto: LoginDto) {
    const response = await this.userService.login(loginDto)
    const tokenInfo: ITokenInfo = {
      accessToken: response.accessToken!,
      refreshToken: response.refreshToken!,
      expiresInSeconds: response.expiresIn!,
      assignDate: Date.now()
    }
    this._setRefreshTimeout(response.expiresIn!)
    this._updateToken(tokenInfo)
    await this.refreshProfileData()
  }

  /** Set timeout which will call refresh auth token */
  private _setRefreshTimeout(timeoutInSeconds: number) {
    if (this.refreshTokenTimeoutId.value) {
      clearTimeout(this.refreshTokenTimeoutId.value)
    }
    this.refreshTokenTimeoutId.value = setTimeout(
      () => this._refreshToken(),
      timeoutInSeconds * 1000
    )
  }

  /** Refresh auth token */
  private async _refreshToken() {
    try {
      const response = await this.userService.refresh(
        new RefreshRequest({
          refreshToken: this.tokenInfo?.refreshToken
        })
      )
      const tokenInfo: ITokenInfo = {
        accessToken: response.accessToken!,
        refreshToken: response.refreshToken!,
        expiresInSeconds: response.expiresIn!,
        assignDate: Date.now()
      }
      this._setRefreshTimeout(response.expiresIn!)
      this._updateToken(tokenInfo)
    } catch {
      this.logout()
    }
  }

  /** Logout by deleting authorization token */
  public async logout() {
    this._updateToken(undefined)
    AuthService.permissionsPromise.value = Promise.resolve([])
    AuthService.permissions.value = []
    AuthService.profileInfo.value = null
    AuthService.profileInfoPromise.value = Promise.resolve(null)
    AuthService.isAuthenticated.value = false
  }

  /** Check if current user has permission */
  static hasPermission(requiresPermission: Permissions) {
    const permissions = AuthService.permissions.value
    if (!AuthService.isAuthenticated.value) {
      return false
    } else {
      return permissions.some((p) => p === Permissions[requiresPermission])
    }
  }
}
