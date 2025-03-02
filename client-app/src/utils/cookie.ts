import type { ICookieOptions } from '@/types/cookie/cookie-options'

/** Set cookie */
export const setCookie = (name: string, value: string, options?: ICookieOptions) => {
  const optionString = options
    ? Object.keys(options).reduce((accStr, key) => {
        const option = options[key as keyof ICookieOptions]
        const isFlag = typeof option === 'boolean'
        if (isFlag && !option) {
          return accStr
        }

        return accStr + (isFlag ? key + ';' : `${key}=${option};`)
      }, '')
    : ''

  //Setting document.cookie to one cookie name value pair does not remove other existing cookies
  const setCookieString = `${name}=${encodeURIComponent(value)};${optionString}`
  document.cookie = setCookieString
}

/** Get page cookie by name. If cookie is not found returns undefined*/
export const getCookie = (name: string) => {
  const cookie = getCookieList().find((c) => c && c[0] == name)
  if (!cookie) {
    return undefined
  }
  return decodeURIComponent(cookie[1])
}

/** Returns list of current page cookie name value pairs */
export const getCookieList = () => {
  //document.cookie contains only cookies names and values in format 'cookieName=cookieValue; cookieName2=cookieValue2'
  return document.cookie
    .split(';')
    .map((c) => {
      const keyValuePair = c.trim().split('=')
      if (keyValuePair.length == 2) {
        return keyValuePair
      } else {
        undefined
      }
    })
    .filter((kvp) => kvp != null)
}
