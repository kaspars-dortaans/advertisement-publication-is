import type { ICookieOptions } from '@/types/cookie/cookie-options'
import { CookieConstants } from './api/CookieConstants'

export const UserSettingCookieOptions: ICookieOptions = {
  'max-age': CookieConstants.MaxSettingCookieAgeInDays * 24 * 60 * 60,
  path: '/',
  samesite: 'none',
  secure: CookieConstants.IsSettingCookieSecure
  //When serving over https set partitioned to true, see: https://developers.google.com/privacy-sandbox/cookies/chips
  //partitioned: true
}
