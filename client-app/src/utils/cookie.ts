import type { ICookieOptions } from '@/types/cookie-options'

/** Set cookie */
export const setCookie = (name: string, value: string, options?: ICookieOptions) => {
  const optionString = options
    ? Object.keys(options).reduce((key) => `${key}=${options[key as keyof ICookieOptions]};`)
    : ''

  //Setting document.cookie to one cookie name value pair does not remove other existing cookies
  document.cookie = `${name}=${encodeURIComponent(value)};${optionString}`
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
