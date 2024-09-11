import { LocaleService } from "@/services/locale-service";
import axios, { AxiosError } from "axios";
import type { App } from "vue";

export const axiosInstance = axios.create()

export const initAxios = (app: App<Element>) => {
    axiosInstance.defaults.baseURL = import.meta.env.VITE_API_URL
    
    const ls = new LocaleService(app.config.globalProperties.$primevue)
    axiosInstance.interceptors.response.use(
        (response) => {
            return response
        }, 
        (error: AxiosError) => {
            let errorMessage = ''
            if(error.response){
                //If error message code present, localize it and return in error to let consumer display it
                if(error.response.data && typeof error.response.data == 'object' && 'messageCode' in error.response.data){
                    errorMessage = ls.l("errors." + error.response.data.messageCode)
                    error.response.data = errorMessage
                }
                
                error.message = errorMessage || error.cause?.message || error.message
            }
            return Promise.reject(error)
        }
    )
}

export default initAxios