import { LocaleService } from '@/services/locale-service'
import axios, { AxiosError, HttpStatusCode } from 'axios'
import type { Router } from 'vue-router'

export const axiosInstance = axios.create()

export const initAxios = (router: Router) => {
  axiosInstance.defaults.baseURL = import.meta.env.VITE_API_URL
  axiosInstance.defaults.withCredentials = true

  const ls = LocaleService.get()
  axiosInstance.interceptors.response.use(
    (response) => {
      return response
    },
    (error: AxiosError) => {
      const response = error?.response
      //If unauthorized redirect to login page
      if (response?.status === HttpStatusCode.Unauthorized) {
        router.push({ name: 'login', query: { redirect: 'true' } })
      } else if (response?.status === HttpStatusCode.NotFound) {
        router.push({ name: 'notFound' })
      }

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
          for (const propertyKey in errors) {
            const propertyErrors = propertyKey.split(/(?<!\[[^[\]]*),+(?![^[\]]*\])/g)
            //Transform collection element keys
            if (propertyErrors.length > 1) {
              const prefix = propertyErrors[0].split('[')[0]
              for (let i = 1; i < propertyErrors.length; i++) {
                propertyErrors[i] = prefix + propertyErrors[i]
              }
            }

            //localize errors
            let deletePropertyKey = true
            for (const propertyErrorKey of propertyErrors) {
              let [errorKey, ...params] = propertyErrorKey.split(
                /(?<!\[[^[\]]*)(?<!\\)\.+(?![^[\]]*\])/g
              )
              errorKey = errorKey.replace(/(?<!\\)\\./, '.')
              params = params.map((p) => p.replace(/(?<!\\)\\./, '.'))

              //convert first char to lowercase for field names to match api-client dto objects field names
              const jsErrorKey = errorKey[0].toLocaleLowerCase() + errorKey.slice(1)
              if (jsErrorKey === propertyKey) {
                deletePropertyKey = false
              }

              //Set first char of field name to be lowercase
              const localizedParams = params.map((p) =>
                ls.l(p.slice(0, 1).toLowerCase() + p.slice(1))
              )
              errors[jsErrorKey] = ls.localizeMultiple(
                errors[propertyKey],
                'errors.',
                ...localizedParams
              )
            }

            if (deletePropertyKey) {
              delete errors[propertyKey]
            }
          }
        }
      }
      return Promise.reject(error)
    }
  )
}

export default initAxios
