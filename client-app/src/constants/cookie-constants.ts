import type { ICookieOptions } from '@/types/cookie/cookie-options'

//TODO: Consider fetching constants from API endpoint
export const UserSettingCookieName = 'userSettings'
export const UserSettingCookieOptions: ICookieOptions = { 'max-age': 7 * 24 * 60 * 60 }
