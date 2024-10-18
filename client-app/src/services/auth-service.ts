import { axiosInstance } from '@/init/axios'
import { getClient } from '@/utils/client-builder'
import { LoginClient, type LoginDto } from './api-client'

export class AuthService {
  readonly client = getClient(LoginClient)
  readonly tokenStorageKey = 'AuthToken'
  jwtToken: string | null = null

  constructor() {
    this.loadStorage()
  }

  loadStorage() {
    this.updateToken(localStorage.getItem(this.tokenStorageKey))
  }

  updateToken(token: string | null) {
    this.jwtToken = token
    this.updateStorage()
    this.updateAuthorizationHeader()
  }

  updateStorage() {
    localStorage.getItem(this.tokenStorageKey)
    if (this.jwtToken) {
      localStorage.setItem(this.tokenStorageKey, this.jwtToken)
    } else {
      localStorage.removeItem(this.tokenStorageKey)
    }
  }

  updateAuthorizationHeader() {
    axiosInstance.defaults.headers.patch.Authorization = this.jwtToken
      ? 'Bearer ' + this.jwtToken
      : undefined
  }

  async login(loginDto: LoginDto) {
    this.updateToken(await this.client.authenticate(loginDto))
  }

  logout() {
    this.updateToken(null)
  }
}
