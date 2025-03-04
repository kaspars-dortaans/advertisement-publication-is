import type { ICookieOptions } from '@/types/cookie/cookie-options'

//TODO: Consider fetching constants from API endpoint
export const UserSettingCookieOptions: ICookieOptions = {
  'max-age': 7 * 24 * 60 * 60,
  path: '/',
  samesite: 'none',
  secure: true
  //When serving over https set partitioned to true, see: https://developers.google.com/privacy-sandbox/cookies/chips
  //partitioned: true
}
