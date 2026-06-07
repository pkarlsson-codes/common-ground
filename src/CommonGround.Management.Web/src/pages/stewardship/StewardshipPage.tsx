import { useState } from 'react'
import type { ReactNode } from 'react'
import StewardshipCasesTab from './StewardshipCasesTab'
import ForeclosurePreventionTab from './ForeclosurePreventionTab'

type TabId = 'leases' | 'fpc'

interface Tab {
  id: TabId
  label: string
}

const TABS: Tab[] = [
  { id: 'leases', label: 'Lease Stewardship' },
  { id: 'fpc', label: 'Foreclosure Prevention' },
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
  const [activeTab, setActiveTab] = useState<TabId>('leases')

  const tabContent: Record<TabId, ReactNode> = {
    leases: <StewardshipCasesTab />,
    fpc: <ForeclosurePreventionTab />,
  }

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <div className="page-header__eyebrow">Homeownership</div>
          <h1 className="page-header__title">Stewardship</h1>
        </div>
      </div>

      <TabBar active={activeTab} onChange={setActiveTab} />

      {tabContent[activeTab]}
    </div>
  )
}
