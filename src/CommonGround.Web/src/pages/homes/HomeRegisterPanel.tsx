import { useState } from 'react'
import { useRegisterHome } from '../../hooks/homes'
import { BUILDING_TYPES, HOME_TYPE_LABELS, type RegisterHomeRequest } from '../../hooks/types'
import EntityCombobox from '../../components/EntityCombobox'
import SlideOver from '../../components/SlideOver'
import { LandParcelsApi } from '../../api'
import { apiConfig } from '../../lib/apiConfig'
import { useToast } from '../../hooks/useToast'

const landParcelsApi = new LandParcelsApi(apiConfig)

const emptyForm = (parcelId = ''): RegisterHomeRequest => ({
  landParcelId: parcelId,
  buildingType: 'SingleFamily',
  bedrooms: 3,
  bathrooms: 2,
  squareFootage: 1200,
  yearBuilt: new Date().getFullYear(),
  initialAssessedValue: 150000,
})

interface Props {
  onClose: () => void
  initialParcelId?: string
}

export default function HomeRegisterPanel({ onClose, initialParcelId }: Props) {
  const [form, setForm] = useState<RegisterHomeRequest>(() => emptyForm(initialParcelId))
  const registerMutation = useRegisterHome()
  const { success } = useToast()

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await registerMutation.mutateAsync({
        ...form,
        bedrooms:             Number(form.bedrooms),
        bathrooms:            Number(form.bathrooms),
        squareFootage:        Number(form.squareFootage),
        yearBuilt:            Number(form.yearBuilt),
        initialAssessedValue: Number(form.initialAssessedValue),
      })
      success('Home registered successfully')
      onClose()
    } catch { /* error shown in form */ }
  }

  return (
    <SlideOver
      title="Register Home"
      subtitle="Add a new home to an existing land parcel"
      onClose={onClose}
      footer={
        <>
          <button className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button
            className="btn btn-primary"
            type="submit"
            form="register-home-form"
            disabled={registerMutation.isPending || !form.landParcelId}
          >
            {registerMutation.isPending ? 'Registering…' : 'Register Home'}
          </button>
        </>
      }
    >
      <form id="register-home-form" onSubmit={handleRegister}>
        <div className="form-grid">
          <div className="form-field full">
            <label className="form-label">Land Parcel</label>
            <EntityCombobox
              value={form.landParcelId || null}
              onChange={id => setForm(f => ({ ...f, landParcelId: id ?? '' }))}
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
            <label className="form-label">Home Type</label>
            <select
              className="form-select"
              value={form.buildingType}
              onChange={e => setForm(f => ({ ...f, buildingType: e.target.value }))}
            >
              {BUILDING_TYPES.map(t => (
                <option key={t} value={t}>{HOME_TYPE_LABELS[t]}</option>
              ))}
            </select>
          </div>
          <div className="form-field">
            <label className="form-label">Bedrooms</label>
            <input
              className="form-input"
              type="number"
              min="0"
              value={form.bedrooms}
              onChange={e => setForm(f => ({ ...f, bedrooms: parseInt(e.target.value) || 0 }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label">Bathrooms</label>
            <input
              className="form-input"
              type="number"
              min="0"
              step="0.5"
              value={form.bathrooms}
              onChange={e => setForm(f => ({ ...f, bathrooms: parseFloat(e.target.value) || 0 }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label">Square Footage</label>
            <input
              className="form-input"
              type="number"
              min="1"
              value={form.squareFootage}
              onChange={e => setForm(f => ({ ...f, squareFootage: parseFloat(e.target.value) || 0 }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label">Year Built</label>
            <input
              className="form-input"
              type="number"
              min="1800"
              max={new Date().getFullYear()}
              value={form.yearBuilt}
              onChange={e => setForm(f => ({ ...f, yearBuilt: parseInt(e.target.value) || 0 }))}
              required
            />
          </div>
          <div className="form-field full">
            <label className="form-label">Initial Assessed Value ($)</label>
            <input
              className="form-input"
              type="number"
              min="0"
              step="1"
              value={form.initialAssessedValue}
              onChange={e => setForm(f => ({ ...f, initialAssessedValue: parseFloat(e.target.value) || 0 }))}
              required
            />
          </div>
        </div>
        {registerMutation.isError && (
          <div className="form-error">Registration failed. Check the parcel ID and try again.</div>
        )}
      </form>
    </SlideOver>
  )
}
