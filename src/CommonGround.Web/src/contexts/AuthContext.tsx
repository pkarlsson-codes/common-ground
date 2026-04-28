import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'

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
    fetch('/api/v1/Auth/me', { credentials: 'include' })
      .then((res) => res.json())
      .then((data) => {
        if (data.success && data.user) {
          setUser(data.user)
        }
      })
      .catch(() => {
        setError('Could not reach the server. Check your connection and reload.')
      })
      .finally(() => setIsLoading(false))
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
