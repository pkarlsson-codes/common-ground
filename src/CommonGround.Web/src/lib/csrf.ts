/** Returns the value of the cg_csrf cookie, or null if absent. */
export function getCsrfToken(): string | null {
  const m = document.cookie.match(/(?:^|;\s*)cg_csrf=([^;]+)/)
  return m ? decodeURIComponent(m[1]) : null
}
