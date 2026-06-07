import { createContext, useContext, type ReactNode } from 'react'
import { useCurrentUser, useRefreshCurrentUser } from '../hooks/auth'

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
  const { data: user = null, isLoading, isError } = useCurrentUser()
  const refreshCurrentUser = useRefreshCurrentUser()

  function logout() {
    window.location.href = '/auth/logout'
  }

  return (
    <AuthContext.Provider value={{
      user,
      isLoading,
      isAuthenticated: !!user,
      error: isError ? 'Could not reach the server. Check your connection and reload.' : null,
      logout,
      refresh: refreshCurrentUser,
    }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
