import type { RouteLocationNormalizedLoaded, Router } from 'vue-router'

export class AppNavigation {
  /** Singleton instance */
  private static _instance: AppNavigation

  /** Router instance */
  private static _router: Router

  /** Remove navigation guard callback */
  private static _removeRouterGuard: () => void

  /** Stores app navigation history.
   *  Does not store navigation history for routes from which user returned with back button.
   *  Last elements contains current route. */
  private _navigationHistory: RouteLocationNormalizedLoaded[] = []

  private _nextIsBackNavigation = false

  private constructor() {
    //Handle browser back button
    addEventListener('popstate', () => {
      this._handleBackNavigation()
    })
  }

  /** Public singleton getter */
  public static get = (router?: Router) => {
    //If no instance create it
    if (!this._instance) {
      this._instance = new AppNavigation()
    }

    //If router was passed assign it and start track its navigation
    if (router) {
      if (this._router && this._removeRouterGuard) {
        this._removeRouterGuard()
      }

      this._router = router
      this._removeRouterGuard = this._router.beforeResolve((to, _, next) => {
        this._instance._navigationHistory.push(to)
        if (this._instance._nextIsBackNavigation) {
          this._instance._nextIsBackNavigation = false
          this._instance._handleBackNavigation()
        }
        next()
      })
    }
    return this._instance
  }

  /** Get last index of route with given path in route navigation history.
   * Search starts from current route or given index.
   */
  public indexOfLastNavigationToPath(path: string, fromIndex?: number) {
    const startIndex =
      typeof fromIndex === 'number' && fromIndex < this._navigationHistory.length
        ? fromIndex
        : this._navigationHistory.length - 1
    for (let i = startIndex; i > -1; i--) {
      if (this._navigationHistory[i].path === path) {
        return i
      }
    }
    return -1
  }

  /** History has previous route (besides current) */
  public hasPrevious() {
    return this._navigationHistory.length > 1
  }

  /** Previous route full path getter */
  public get getPreviousFullPath() {
    return this._navigationHistory[this._navigationHistory.length - 2].fullPath
  }

  /** Current route path getter */
  public get getCurrentPath() {
    return this._navigationHistory[this._navigationHistory.length - 1].fullPath
  }

  /** Mark next route navigation as being 'back navigation' - navigation made with back button */
  public setNextAsBackNavigation() {
    this._nextIsBackNavigation = true
  }

  /**
   * Remove navigation from which user returned with back button
   */
  private _handleBackNavigation() {
    const historyLength = this._navigationHistory.length

    if (historyLength === 0) {
      return
    }
    const previousNavigation = this._navigationHistory[historyLength - 1]
    const toLastNavigationIndex = this.indexOfLastNavigationToPath(
      previousNavigation.path,
      this._navigationHistory.length - 2
    )
    this._navigationHistory.splice(toLastNavigationIndex + 1)
  }
}
