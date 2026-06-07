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

export const apiConfig = new Configuration({
  basePath: '',
  credentials: 'include',
  middleware: [csrfMiddleware],
})
