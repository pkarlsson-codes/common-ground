import PageEyebrow from '../../components/PageEyebrow'
import { useEffect, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import type { ReactNode } from 'react'
import StewardshipCasesTab from './StewardshipCasesTab'
import ForeclosurePreventionTab, { type OpenCaseRequest } from './ForeclosurePreventionTab'
import LeaseFeesTab from './LeaseFeesTab'
import ResalePricesTab from './ResalePricesTab'

type TabId = 'leases' | 'fpc' | 'fees' | 'resale'

interface Tab {
  id: TabId
  label: string
}

const TABS: Tab[] = [
  { id: 'leases', label: 'Lease Stewardship' },
  { id: 'fpc', label: 'Foreclosure Prevention' },
  { id: 'fees', label: 'Lease Fees' },
  { id: 'resale', label: 'Resale Prices' },
]

function TabBar({
  active,
  onChange,
}: {
  active: TabId
  onChange: (id: TabId) => void
}) {
  return (
    <div
      style={{
        display: 'flex',
        borderBottom: '1px solid var(--border)',
        marginBottom: 24,
        gap: 0,
      }}
    >
      {TABS.map(tab => (
        <button
          key={tab.id}
          type="button"
          onClick={() => onChange(tab.id)}
          style={{
            padding: '10px 20px',
            background: 'transparent',
            border: 'none',
            borderBottom: active === tab.id
              ? '2px solid var(--amber-bright)'
              : '2px solid transparent',
            color: active === tab.id ? 'var(--text-bright)' : 'var(--text-secondary)',
            fontFamily: 'var(--font-body)',
            fontSize: 13,
            fontWeight: active === tab.id ? 600 : 400,
            cursor: 'pointer',
            marginBottom: -1,
            transition: 'color var(--transition), border-color var(--transition)',
          }}
        >
          {tab.label}
        </button>
      ))}
    </div>
  )
}

export default function StewardshipPage() {
  const location = useLocation()
  const navigate = useNavigate()
  const [activeTab, setActiveTab] = useState<TabId>('leases')
  const [fpcRequest, setFpcRequest] = useState<OpenCaseRequest | null>(null)

  // Another page opens a foreclosure prevention case here with navigate('/stewardship', { state: { openFpcId } }).
  // Each navigation has its own location key, so following the same link twice opens the case twice.
  const requestedFpcId = (location.state as { openFpcId?: string } | null)?.openFpcId
  if (requestedFpcId && fpcRequest?.key !== location.key) {
    setActiveTab('fpc')
    setFpcRequest({ id: requestedFpcId, key: location.key })
  }

  useEffect(() => {
    if (!requestedFpcId) return
    navigate({ pathname: location.pathname, search: location.search, hash: location.hash }, { replace: true, state: null })
  }, [requestedFpcId, location, navigate])

  const tabContent: Record<TabId, ReactNode> = {
    leases: <StewardshipCasesTab />,
    fpc: <ForeclosurePreventionTab openRequest={fpcRequest} />,
    fees: <LeaseFeesTab />,
    resale: <ResalePricesTab />,
  }

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <PageEyebrow />
          <h1 className="page-header__title">Stewardship</h1>
        </div>
      </div>

      <TabBar active={activeTab} onChange={setActiveTab} />

      {tabContent[activeTab]}
    </div>
  )
}
