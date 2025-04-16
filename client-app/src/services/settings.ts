import { CookieConstants } from '@/constants/api/CookieConstants'
import { UserSettingCookieOptions } from '@/constants/cookie-constants'
import type { ICookieUserSettings } from '@/types/cookie/cookie-user-settings'
import { getCookie, setCookie } from '@/utils/cookie'
import { reactive, watch, type Reactive } from 'vue'

export class Settings {
  private static _instance: Settings
  public userSettingCookieValue: Reactive<ICookieUserSettings>

  /** Singleton getter */
  static get() {
    if (!this._instance) {
      this._instance = new Settings()
    }
    return this._instance
  }

  private constructor() {
    //Get and set settings from cookie
    const settingCookieValue = getCookie(CookieConstants.UserSettingCookieName)
    const valueObject = settingCookieValue ? (JSON.parse(settingCookieValue) as object) : {}

    const settings = {
      locale: 'locale' in valueObject && valueObject.locale ? valueObject.locale : '',
      timeZoneId: Intl.DateTimeFormat().resolvedOptions().timeZone
    } as ICookieUserSettings

    this.userSettingCookieValue = reactive(settings)

    //On settings change sync cookie value
    watch(
      this.userSettingCookieValue,
      (value) => {
        this.syncCookieValue(value)
      },
      { immediate: true }
    )
  }

  private syncCookieValue(value: ICookieUserSettings) {
    const settingValue = JSON.stringify(value)
    setCookie(CookieConstants.UserSettingCookieName, settingValue, UserSettingCookieOptions)
  }
}
