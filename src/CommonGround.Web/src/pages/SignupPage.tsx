import { useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { TrustsApi } from '../api'
import { useAuth } from '../contexts/AuthContext'
import { ResponseError } from '../api/runtime'
import { apiConfig } from '../lib/apiConfig'
import { getErrorMessage } from '../lib/getErrorMessage'

const trustsApi = new TrustsApi(apiConfig)

export default function SignupPage() {
  const [name, setName] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const { refresh } = useAuth()
  const navigate = useNavigate()

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const trimmed = name.trim()
    if (!trimmed) {
      setError('Please enter a community land trust name.')
      return
    }

    setIsSubmitting(true)
    setError(null)

    try {
      await trustsApi.createTrust({ createTrustRequest: { name: trimmed } })

      await refresh()
      navigate('/', { replace: true })
    } catch (error) {
      if (error instanceof ResponseError && error.response.status === 409) {
        setError('You already belong to a trust.')
        return
      }

      setError(await getErrorMessage(error, 'Could not create trust. Please try again.'))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div style={{
      minHeight: '100vh',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      background: 'var(--bg)',
      padding: '24px',
    }}>
      <form
        onSubmit={onSubmit}
        style={{
          textAlign: 'left',
          padding: '42px 44px',
          background: 'var(--surface)',
          border: '1px solid var(--border)',
          borderRadius: '12px',
          maxWidth: '440px',
          width: '100%',
          display: 'flex',
          flexDirection: 'column',
          gap: '18px',
        }}
      >
        <div>
          <div style={{
            fontFamily: 'var(--font-mono)',
            fontSize: '9px',
            letterSpacing: '0.22em',
            textTransform: 'uppercase',
            color: 'var(--amber)',
            marginBottom: '10px',
          }}>
            New CLT
          </div>
          <h1 style={{
            margin: 0,
            fontFamily: 'var(--font-display)',
            fontSize: '30px',
            lineHeight: 1.15,
            color: 'var(--text-bright)',
          }}>
            Create your trust
          </h1>
          <p style={{
            margin: '10px 0 0 0',
            fontSize: '13px',
            color: 'var(--text-dim)',
            lineHeight: 1.5,
          }}>
            Start with your organization name. You will be brought into the app once your trust is created.
          </p>
        </div>

        <label style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
          <span style={{ fontSize: '12px', letterSpacing: '0.05em', textTransform: 'uppercase', color: 'var(--text-dim)' }}>
            Trust Name
          </span>
          <input
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder='Example Community Land Trust'
            autoFocus
            disabled={isSubmitting}
            style={{
              width: '100%',
              padding: '12px 14px',
              borderRadius: '8px',
              border: '1px solid var(--border)',
              background: 'transparent',
              color: 'var(--text-bright)',
              fontSize: '14px',
              fontFamily: 'var(--font-body)',
            }}
          />
        </label>

        {error && (
          <div style={{ color: 'var(--error-bright)', fontSize: '13px' }}>{error}</div>
        )}

        <button
          type='submit'
          disabled={isSubmitting}
          style={{
            width: '100%',
            padding: '11px 20px',
            background: 'var(--amber)',
            color: 'var(--bg)',
            border: 'none',
            borderRadius: '6px',
            fontFamily: 'var(--font-body)',
            fontSize: '13.5px',
            fontWeight: 600,
            cursor: isSubmitting ? 'default' : 'pointer',
            letterSpacing: '0.02em',
            opacity: isSubmitting ? 0.7 : 1,
          }}
        >
          {isSubmitting ? 'Creating trust…' : 'Create trust and continue'}
        </button>
      </form>
    </div>
  )
}