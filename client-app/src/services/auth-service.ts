import { AuthHeaderName, TokenStorageKey } from '@/constants/auth'
import { axiosInstance } from '@/init/axios'
import { getClient } from '@/utils/client-builder'
import { ref, type Ref } from 'vue'
import { UserClient, type LoginDto } from './api-client'

export class AuthService {
  /** Singleton instance */
  private static _instance: AuthService

  /** Current user id */
  public static readonly permissions: Ref<Promise<string[]>> = ref(Promise.resolve([]))

  /** Is user authenticated flag */
  public static readonly isAuthenticated: Ref<boolean> = ref(false)

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
      await this._loadPermissions()
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

  private async _loadPermissions() {
    AuthService.permissions.value = this.userService.getCurrentUserPermissions()
  }

  /** Attempt to login, via sending login request to Api */
  async login(loginDto: LoginDto) {
    const newTokenPromise = this.userService.authenticate(loginDto)
    const loadPermissionPromise = this._loadPermissions()

    this._updateToken(await newTokenPromise)
    await loadPermissionPromise

    AuthService.isAuthenticated.value = true
  }

  /** Logout by deleting authorization jwt token */
  logout() {
    this._updateToken(null)
    AuthService.permissions.value = Promise.resolve([])
    AuthService.isAuthenticated.value = false
  }

  static async hasPermission(requiredPermission: string) {
    const permissions = await AuthService.permissions.value
    if (!requiredPermission) {
      return true
    } else if (!AuthService.isAuthenticated.value) {
      return false
    } else {
      return permissions.some((p) => p === requiredPermission)
    }
  }
}
