export interface ICookieOptions {
  path?: string
  expirationDate?: Date
  domain?: string
  samesite?: 'lax' | 'strict' | 'none'
  'max-age'?: number //seconds
  secure?: boolean
}
