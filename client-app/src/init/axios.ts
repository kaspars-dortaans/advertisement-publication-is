import { LocaleService } from '@/services/locale-service'
import axios, { AxiosError } from 'axios'
import type { App } from 'vue'

export const axiosInstance = axios.create()

export const initAxios = (app: App<Element>) => {
  axiosInstance.defaults.baseURL = import.meta.env.VITE_API_URL

  const ls = LocaleService.get(app.config.globalProperties.$primevue)
  axiosInstance.interceptors.response.use(
    (response) => {
      return response
    },
    (error: AxiosError) => {
      //If custom error codes where returned, localize them
      const data = error?.response?.data
      if (data && typeof data == 'object') {
        //Endpoint errors
        if ('errorCodes' in data && Array.isArray(data.errorCodes)) {
          data.errorCodes = data.errorCodes.map((code) => ls.l('errors.' + code))
        }

        //Dto validation errors
        if ('errors' in data && data.errors && typeof data.errors == 'object') {
          const errors = data.errors as { [k: string]: string[] }
          for (const key in errors) {
            const [errorKey, ...params] = key.split('.')
            //Set first char of field name to be lowercase
            const localizedParams = params.map((p) =>
              ls.l(p.slice(0, 1).toLowerCase() + p.slice(1))
            )
            errors[errorKey] = ls.localizeMultiple(errors[key], 'errors.', ...localizedParams)
          }
        }
      }
      return Promise.reject(error)
    }
  )
}

export default initAxios
