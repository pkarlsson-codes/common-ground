import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from 'react'
import { AuthApi } from '../api'
import { apiConfig } from '../lib/apiConfig'

interface User {
  id: string
  username: string
  email: string
  trustId?: string | null
  hasTrust: boolean
}

interface AuthContextValue {
  user: User | null
  isLoading: boolean
  isAuthenticated: boolean
  error: string | null
  logout: () => void
  refresh: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const loadUser = useCallback(async () => {
    try {
      const api = new AuthApi(apiConfig)
      const data = await api.getCurrentUser()
      if (data && data.id) {
        const hasTrust = data.hasTrust ?? !!data.trustId
        setUser({
          id: data.id,
          username: data.username ?? data.id,
          email: data.email ?? '',
          trustId: data.trustId,
          hasTrust,
        })
        setError(null)
      } else {
        setUser(null)
      }
    } catch {
      setUser(null)
      setError('Could not reach the server. Check your connection and reload.')
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadUser()
  }, [loadUser])

  function logout() {
    window.location.href = '/auth/logout'
  }

  return (
    <AuthContext.Provider value={{ user, isLoading, isAuthenticated: !!user, error, logout, refresh: loadUser }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
