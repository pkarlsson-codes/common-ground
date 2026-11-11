import { Configuration, type Middleware } from '../api'
import { getCsrfToken } from './csrf'

const csrfMiddleware: Middleware = {
  pre: async (ctx) => {
    const method = (ctx.init.method ?? 'GET').toUpperCase()
    if (!['GET', 'HEAD', 'OPTIONS'].includes(method)) {
      const token = getCsrfToken()
      if (token) {
        ctx.init.headers = { ...ctx.init.headers, 'X-CSRF-Token': token }
      }
    }
    return ctx
  },
}

// The API refuses every request from a user removed from the trust, and the gateway has already
// dropped the trust from the session. Reloading sends them to onboarding.
const membershipEndedMiddleware: Middleware = {
  post: async (ctx) => {
    if (ctx.response.status === 403) {
      const body = await ctx.response.clone().json().catch(() => null) as { code?: string } | null
      if (body?.code === 'identity.trust_membership.ended') window.location.assign('/')
    }
    return ctx.response
  },
}

export const apiConfig = new Configuration({
  basePath: '',
  credentials: 'include',
  middleware: [csrfMiddleware, membershipEndedMiddleware],
})
