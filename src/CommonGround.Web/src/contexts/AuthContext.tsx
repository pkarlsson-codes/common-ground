import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { AuthApi } from '../api'
import { apiConfig } from '../lib/apiConfig'

interface User {
  id: string
  username: string
  email: string
}

interface AuthContextValue {
  user: User | null
  isLoading: boolean
  isAuthenticated: boolean
  error: string | null
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const loadUser = async () => {
      try {
        const api = new AuthApi(apiConfig)
        const data = await api.getCurrentUser()
        if (data && data.id) {
          setUser({ id: data.id, username: data.username ?? data.id, email: data.email ?? '' })
        }
      } catch {
        setError('Could not reach the server. Check your connection and reload.')
      } finally {
        setIsLoading(false)
      }
    }

    void loadUser()
  }, [])

  function logout() {
    window.location.href = '/auth/logout'
  }

  return (
    <AuthContext.Provider value={{ user, isLoading, isAuthenticated: !!user, error, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
