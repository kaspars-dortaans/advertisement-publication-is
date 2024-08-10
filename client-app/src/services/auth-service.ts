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
        const storage = new Storage()
        this.updateToken(storage.getItem(this.tokenStorageKey))
    }

    updateToken(token: string | null) {
        this.jwtToken = token
        this.updateStorage()
        this.updateAuthorizationHeader()
    }
    
    updateStorage() {
        const storage = new Storage()
        storage.getItem(this.tokenStorageKey)
        if(this.jwtToken){
            storage.setItem(this.tokenStorageKey, this.jwtToken)
        } else {
            storage.removeItem(this.tokenStorageKey)
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