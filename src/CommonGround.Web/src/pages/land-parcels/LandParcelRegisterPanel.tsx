import { useState } from 'react'
import { useRegisterLandParcel } from '../../hooks/landParcels'
import { ACQUISITION_METHODS, type RegisterLandParcelRequest } from '../../hooks/types'
import SlideOver from '../../components/SlideOver'
import { useToast } from '../../hooks/useToast'

const emptyForm = (): RegisterLandParcelRequest => ({
  legalParcelNumber: '',
  address: '',
  areaAcres: 0,
  acquisitionDate: new Date().toISOString().slice(0, 10),
  acquisitionMethod: 'Purchase',
  covenantTerms: '',
})

interface Props {
  onClose: () => void
}

export default function LandParcelRegisterPanel({ onClose }: Props) {
  const [form, setForm] = useState<RegisterLandParcelRequest>(emptyForm)
  const registerMutation = useRegisterLandParcel()
  const { success } = useToast()

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await registerMutation.mutateAsync({
        ...form,
        areaAcres: Number(form.areaAcres),
        covenantTerms: form.covenantTerms || undefined,
      })
      success('Land parcel registered successfully')
      onClose()
    } catch { /* error shown in form */ }
  }

  return (
    <SlideOver
      title="Register Land Parcel"
      subtitle="Add a new parcel to the CLT registry"
      onClose={onClose}
      footer={
        <>
          <button className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button
            className="btn btn-primary"
            type="submit"
            form="register-parcel-form"
            disabled={registerMutation.isPending}
          >
            {registerMutation.isPending ? 'Registering…' : 'Register Parcel'}
          </button>
        </>
      }
    >
      <form id="register-parcel-form" onSubmit={handleRegister}>
        <div className="form-grid">
          <div className="form-field full">
            <label className="form-label">Legal Parcel Number</label>
            <input
              className="form-input"
              value={form.legalParcelNumber}
              onChange={e => setForm(f => ({ ...f, legalParcelNumber: e.target.value }))}
              placeholder="e.g. 123-456-789"
              required
            />
          </div>
          <div className="form-field full">
            <label className="form-label">Address</label>
            <input
              className="form-input"
              value={form.address}
              onChange={e => setForm(f => ({ ...f, address: e.target.value }))}
              placeholder="Street address"
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label">Area (acres)</label>
            <input
              className="form-input"
              type="number"
              step="0.0001"
              min="0.0001"
              value={form.areaAcres || ''}
              onChange={e => setForm(f => ({ ...f, areaAcres: parseFloat(e.target.value) || 0 }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label">Acquisition Date</label>
            <input
              className="form-input"
              type="date"
              value={form.acquisitionDate}
              onChange={e => setForm(f => ({ ...f, acquisitionDate: e.target.value }))}
              required
            />
          </div>
          <div className="form-field full">
            <label className="form-label">Acquisition Method</label>
            <select
              className="form-select"
              value={form.acquisitionMethod}
              onChange={e => setForm(f => ({ ...f, acquisitionMethod: e.target.value }))}
            >
              {ACQUISITION_METHODS.map(m => <option key={m} value={m}>{m}</option>)}
            </select>
          </div>
          <div className="form-field full">
            <label className="form-label">Covenant Terms (optional)</label>
            <textarea
              className="form-textarea"
              value={form.covenantTerms}
              onChange={e => setForm(f => ({ ...f, covenantTerms: e.target.value }))}
              placeholder="Any restrictions or covenant conditions…"
            />
          </div>
        </div>
        {registerMutation.isError && (
          <div className="form-error">Registration failed. Is the API running?</div>
        )}
      </form>
    </SlideOver>
  )
}
