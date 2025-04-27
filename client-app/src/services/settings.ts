import { CookieConstants } from '@/constants/api/CookieConstants'
import { UserSettingCookieOptions } from '@/constants/cookie-constants'
import type { ICookieUserSettings } from '@/types/cookie/cookie-user-settings'
import { getCookie, setCookie } from '@/utils/cookie'
import { ref, watch, type Ref } from 'vue'
import { AuthService } from './auth-service'

export class Settings {
  private static _instance: Settings
  public userSettingCookieValue: Ref<ICookieUserSettings>

  /** Singleton getter */
  static get() {
    if (!this._instance) {
      this._instance = new Settings()
    }
    return this._instance
  }

  private constructor() {
    //Get and set settings from cookie
    this.userSettingCookieValue = ref(this.readCookie())

    //On settings change sync cookie value
    watch(
      this.userSettingCookieValue,
      (value) => {
        this.syncCookieValue(value)
      },
      { immediate: true }
    )
    watch(AuthService.isAuthenticated, () => {
      this.userSettingCookieValue.value = this.readCookie()
    })
  }

  private readCookie = () => {
    const settingCookieValue = getCookie(CookieConstants.UserSettingCookieName)
    const valueObject = settingCookieValue ? (JSON.parse(settingCookieValue) as object) : {}

    return {
      locale: 'locale' in valueObject && valueObject.locale ? valueObject.locale : '',
      timeZoneId: Intl.DateTimeFormat().resolvedOptions().timeZone
    } as ICookieUserSettings
  }

  private syncCookieValue(value: ICookieUserSettings) {
    const settingValue = JSON.stringify(value)
    setCookie(CookieConstants.UserSettingCookieName, settingValue, UserSettingCookieOptions)
  }
}
