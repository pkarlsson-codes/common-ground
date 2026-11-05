import { useState } from 'react'
import { HomesApi, LandParcelsApi } from '../../api'
import EntityCombobox from '../../components/EntityCombobox'
import SlideOver from '../../components/SlideOver'
import { useCreateGroundLease } from '../../hooks/groundLeases'
import {
  FORMULA_KIND_LABELS,
  FORMULA_KINDS,
  INDEX_TYPES,
  type CreateGroundLeaseForm,
} from '../../hooks/types'
import { useToast } from '../../hooks/useToast'
import { apiConfig } from '../../lib/apiConfig'
import { getErrorMessage } from '../../lib/getErrorMessage'
import { trustToday } from '../../lib/calendarDate'
import { DateInput } from '../../components/DateInput'
import { formatPhoneInput, isValidPhone } from '../../lib/phone'

const landParcelsApi = new LandParcelsApi(apiConfig)
const homesApi = new HomesApi(apiConfig)

const emptyCreate = (): CreateGroundLeaseForm => ({
  landParcelId: '',
  homeId: '',
  homeownerFullName: '',
  homeownerEmail: '',
  homeownerPhone: '',
  leaseStartDate: trustToday(),
  termYears: 99,
  covenantTerms: '',
  formulaKind: 'AppraisalBased',
  appreciationShare: 0.25,
  indexType: 'CPI',
  baselineIndexValue: 100,
  annualPercentage: 0.01,
  improvementsCap: 25000,
  hasImprovementsCap: false,
})

interface Props {
  onClose: () => void
}

export default function GroundLeaseCreatePanel({ onClose }: Props) {
  const [createForm, setCreateForm] = useState<CreateGroundLeaseForm>(emptyCreate)
  const [createError, setCreateError] = useState<string | null>(null)
  const createMutation = useCreateGroundLease()
  const { success } = useToast()

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault()
    setCreateError(null)
    if (createForm.homeownerPhone && !isValidPhone(createForm.homeownerPhone)) {
      setCreateError('Enter a valid phone number, or leave it blank.')
      return
    }
    try {
      await createMutation.mutateAsync(createForm)
      onClose()
      success('Ground lease created')
    } catch (error) {
      setCreateError(await getErrorMessage(error, 'Failed to create ground lease.'))
    }
  }

  return (
    <SlideOver
      title="Create Ground Lease"
      subtitle="Register a new community land trust ground lease"
      onClose={onClose}
      footer={
        <>
          <button className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button
            className="btn btn-primary"
            type="submit"
            form="create-ground-lease-form"
            disabled={createMutation.isPending || !createForm.landParcelId || !createForm.homeId}
          >
            {createMutation.isPending ? 'Creating…' : 'Create Lease'}
          </button>
        </>
      }
    >
      <form id="create-ground-lease-form" onSubmit={handleCreate}>
        <div className="form-grid">
          <div className="form-field full">
            <label className="form-label">Land Parcel</label>
            <EntityCombobox
              value={createForm.landParcelId || null}
              onChange={id => setCreateForm(f => ({ ...f, landParcelId: id ?? '', homeId: '' }))}
              fetchOptions={async q => {
                const result = await landParcelsApi.getLandParcels({ search: q, pageSize: 10 })
                return (result.items ?? []).map(p => ({
                  id: p.id!,
                  label: [p.legalParcelNumber, p.address].filter(Boolean).join(' — '),
                }))
              }}
              placeholder="Search parcels…"
            />
          </div>
          <div className="form-field full">
            <label className="form-label">Home</label>
            <EntityCombobox
              value={createForm.homeId || null}
              onChange={id => setCreateForm(f => ({ ...f, homeId: id ?? '' }))}
              fetchOptions={async q => {
                const result = await homesApi.getHomes({ landParcelIds: [createForm.landParcelId], pageSize: 10 })
                const lower = q.toLowerCase()
                const items = result.items ?? []
                const filtered = lower ? items.filter(b =>
                  `${b.homeType} ${b.bedrooms} ${b.bathrooms} ${b.squareFootage}`.toLowerCase().includes(lower)
                ) : items
                return filtered.map(b => ({
                  id: b.id!,
                  label: `${b.homeType}, ${b.bedrooms}bd/${b.bathrooms}ba, ${b.squareFootage} sqft`,
                }))
              }}
              placeholder={createForm.landParcelId ? 'Search homes on this parcel…' : 'Select a parcel first'}
              disabled={!createForm.landParcelId}
            />
          </div>
          <div className="form-field full">
            <label className="form-label">Homeowner Full Name</label>
            <input
              className="form-input"
              value={createForm.homeownerFullName}
              onChange={e => setCreateForm(f => ({ ...f, homeownerFullName: e.target.value }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label">Homeowner Email</label>
            <input
              className="form-input"
              type="email"
              value={createForm.homeownerEmail}
              onChange={e => setCreateForm(f => ({ ...f, homeownerEmail: e.target.value }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label">Homeowner Phone (optional)</label>
            <input
              className="form-input"
              type="tel"
              placeholder="(202) 555-1234"
              value={createForm.homeownerPhone}
              onChange={e => {
                const next = formatPhoneInput(e.target.value, createForm.homeownerPhone)
                setCreateForm(f => ({ ...f, homeownerPhone: next }))
              }}
            />
          </div>
          <div className="form-field">
            <label className="form-label">Lease Start Date</label>
            <DateInput
              className="form-input"
              value={createForm.leaseStartDate}
              onChange={v => setCreateForm(f => ({ ...f, leaseStartDate: v }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label">Term (years)</label>
            <input
              className="form-input"
              type="number"
              min="1"
              max="999"
              value={createForm.termYears}
              onChange={e => setCreateForm(f => ({ ...f, termYears: parseInt(e.target.value) || 99 }))}
              required
            />
          </div>
          <div className="form-field full">
            <label className="form-label">Covenant Terms</label>
            <textarea
              className="form-textarea"
              value={createForm.covenantTerms}
              onChange={e => setCreateForm(f => ({ ...f, covenantTerms: e.target.value }))}
              placeholder="Describe the covenant restrictions…"
              required
            />
          </div>

          <div className="form-field full">
            <label className="form-label">Resale Formula</label>
            <select
              className="form-select"
              value={createForm.formulaKind}
              onChange={e => setCreateForm(f => ({ ...f, formulaKind: e.target.value }))}
            >
              {FORMULA_KINDS.map(k => (
                <option key={k} value={k}>{FORMULA_KIND_LABELS[k]}</option>
              ))}
            </select>
          </div>

          {createForm.formulaKind === 'AppraisalBased' && (
            <div className="form-field">
              <label className="form-label">Appreciation Share</label>
              <input
                className="form-input"
                type="number"
                min="0"
                max="1"
                step="0.01"
                value={createForm.appreciationShare}
                onChange={e => setCreateForm(f => ({ ...f, appreciationShare: parseFloat(e.target.value) || 0 }))}
                required
              />
              <div className="form-help">Decimal fraction, e.g. 0.25 = 25%</div>
            </div>
          )}

          {createForm.formulaKind === 'IndexBased' && (
            <>
              <div className="form-field">
                <label className="form-label">Index Type</label>
                <select
                  className="form-select"
                  value={createForm.indexType}
                  onChange={e => setCreateForm(f => ({ ...f, indexType: e.target.value }))}
                >
                  {INDEX_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
                </select>
              </div>
              <div className="form-field">
                <label className="form-label">Baseline Index Value</label>
                <input
                  className="form-input"
                  type="number"
                  min="0"
                  step="0.01"
                  value={createForm.baselineIndexValue}
                  onChange={e => setCreateForm(f => ({ ...f, baselineIndexValue: parseFloat(e.target.value) || 0 }))}
                  required
                />
              </div>
            </>
          )}

          {createForm.formulaKind === 'FixedPercentage' && (
            <div className="form-field">
              <label className="form-label">Annual Percentage</label>
              <input
                className="form-input"
                type="number"
                min="0"
                max="1"
                step="0.001"
                value={createForm.annualPercentage}
                onChange={e => setCreateForm(f => ({ ...f, annualPercentage: parseFloat(e.target.value) || 0 }))}
                required
              />
              <div className="form-help">Decimal fraction, e.g. 0.03 = 3%</div>
            </div>
          )}

          <div className="form-field full" style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <input
              type="checkbox"
              id="has-cap"
              checked={createForm.hasImprovementsCap}
              onChange={e => setCreateForm(f => ({ ...f, hasImprovementsCap: e.target.checked }))}
            />
            <label htmlFor="has-cap" className="form-label" style={{ marginBottom: 0 }}>
              Set improvements cap
            </label>
          </div>

          {createForm.hasImprovementsCap && (
            <div className="form-field">
              <label className="form-label">Improvements Cap ($)</label>
              <input
                className="form-input"
                type="number"
                min="0"
                step="1"
                value={createForm.improvementsCap}
                onChange={e => setCreateForm(f => ({ ...f, improvementsCap: parseFloat(e.target.value) || 0 }))}
                required
              />
            </div>
          )}
        </div>
        {createError && (
          <div className="form-error">{createError}</div>
        )}
      </form>
    </SlideOver>
  )
}