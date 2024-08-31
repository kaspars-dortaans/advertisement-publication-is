import axios from "axios"
import { Client, type LoginDto } from "./api-client"

export class AuthService {
    readonly client = new Client()
    readonly tokenStorageKey = 'AuthToken'
    jwtToken: string | null = null

    constructor(){
        this.loadStorage()
    }
    
    loadStorage () {
        this.updateToken(localStorage.getItem(this.tokenStorageKey))
    }

    updateToken(token: string | null) {
        this.jwtToken = token
        this.updateStorage()
        this.updateAuthorizationHeader()
    }
    
    updateStorage() {
        localStorage.getItem(this.tokenStorageKey)
        if(this.jwtToken){
            localStorage.setItem(this.tokenStorageKey, this.jwtToken)
        } else {
            localStorage.removeItem(this.tokenStorageKey)
        }
    }
    
    updateAuthorizationHeader() {
        axios.defaults.headers.patch.Authorization = this.jwtToken ? 'Bearer ' + this.jwtToken : undefined
    }
    
    async login(loginDto: LoginDto) {
        this.updateToken(await this.client.login(loginDto))
    }
    
    logout() {
        this.updateToken(null)
    }
}