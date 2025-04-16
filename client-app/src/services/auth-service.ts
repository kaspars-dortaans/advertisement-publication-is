import { AuthHeaderName, TokenStorageKey } from '@/constants/auth'
import { axiosInstance } from '@/init/axios'
import { getClient } from '@/utils/client-builder'
import { ref } from 'vue'
import { UserClient, UserInfo, type LoginDto } from './api-client'

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

  /** Authentication jwt token */
  jwtToken: string | null = null

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
    this._updateToken(localStorage.getItem(TokenStorageKey))
    if (this.jwtToken) {
      AuthService.isAuthenticated.value = true
      await this.refreshProfileData()
    }
  }

  /** Update stored authentication token */
  private _updateToken(token: string | null) {
    this.jwtToken = token
    this._updateStorage()
    this._updateAuthorizationHeader()
  }

  /** Sync stored authentication data in locale storage */
  private _updateStorage() {
    localStorage.getItem(TokenStorageKey)
    if (this.jwtToken) {
      localStorage.setItem(TokenStorageKey, this.jwtToken)
    } else {
      localStorage.removeItem(TokenStorageKey)
    }
  }

  /** Update authorization header value for axios requests */
  private _updateAuthorizationHeader() {
    axiosInstance.defaults.headers.common[AuthHeaderName] = this.jwtToken
      ? 'Bearer ' + this.jwtToken
      : undefined
  }

  public async refreshProfileData() {
    try {
      AuthService.permissionsPromise.value = this.userService.getCurrentUserPermissions()
      AuthService.profileInfoPromise.value = this.userService.getUserInfo()
      AuthService.permissions.value = await AuthService.permissionsPromise.value
      AuthService.profileInfo.value = await AuthService.profileInfoPromise.value
      AuthService.isAuthenticated.value = true
    } catch (e) {
      //TODO: Implement token refresh & try to refresh token
      this.logout()
    }
  }

  /** Attempt to login, via sending login request to Api */
  async login(loginDto: LoginDto) {
    const newToken = await this.userService.authenticate(loginDto)
    this._updateToken(newToken)

    await this.refreshProfileData()
  }

  /** Logout by deleting authorization jwt token */
  public logout() {
    this._updateToken(null)
    AuthService.permissionsPromise.value = Promise.resolve([])
    AuthService.permissions.value = []
    AuthService.profileInfo.value = null
    AuthService.profileInfoPromise.value = Promise.resolve(null)
    AuthService.isAuthenticated.value = false
  }

  static async hasPermission(requiresPermission: string) {
    const permissions = await AuthService.permissionsPromise.value
    if (!requiresPermission) {
      return true
    } else if (!AuthService.isAuthenticated.value) {
      return false
    } else {
      return permissions.some((p) => p === requiresPermission)
    }
  }
}
