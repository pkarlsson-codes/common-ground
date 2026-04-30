import { useState } from 'react'
import { useInitiateDevelopmentProject } from '../../hooks/developmentProjects'
import { useHomes } from '../../hooks/homes'
import { HOME_TYPE_LABELS, type InitiateDevelopmentProjectRequest } from '../../hooks/types'
import EntityCombobox from '../../components/EntityCombobox'
import SlideOver from '../../components/SlideOver'
import { LandParcelsApi } from '../../api'
import { apiConfig } from '../../lib/apiConfig'
import { useToast } from '../../hooks/useToast'

const landParcelsApi = new LandParcelsApi(apiConfig)

const emptyForm = (parcelId = ''): InitiateDevelopmentProjectRequest => ({
  landParcelId: parcelId,
  name: '',
  description: '',
  projectedStartDate: '',
  projectedEndDate: '',
  budget: undefined,
  homeIds: [],
})

interface Props {
  onClose: () => void
  initialParcelId?: string
}

export default function DevelopmentProjectInitiatePanel({ onClose, initialParcelId }: Props) {
  const [form, setForm] = useState<InitiateDevelopmentProjectRequest>(() => emptyForm(initialParcelId))
  const initiateMutation = useInitiateDevelopmentProject()
  const { success } = useToast()

  const { data: homesData } = useHomes({
    page: 1,
    pageSize: 100,
    landParcelId: form.landParcelId || undefined,
  })
  const parcelHomes = homesData?.items ?? []

  const toggleHomeId = (id: string) => {
    setForm(f => {
      const ids = f.homeIds ?? []
      return {
        ...f,
        homeIds: ids.includes(id) ? ids.filter(x => x !== id) : [...ids, id],
      }
    })
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await initiateMutation.mutateAsync({
        ...form,
        projectedStartDate: form.projectedStartDate || undefined,
        projectedEndDate: form.projectedEndDate || undefined,
        description: form.description || undefined,
        budget: form.budget ? Number(form.budget) : undefined,
        homeIds: form.homeIds?.length ? form.homeIds : undefined,
      })
      success('Development project initiated')
      onClose()
    } catch { /* error shown in form */ }
  }

  return (
    <SlideOver
      title="Initiate Development Project"
      subtitle="Start a new development project on a land parcel"
      onClose={onClose}
      footer={
        <>
          <button className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button
            className="btn btn-primary"
            type="submit"
            form="initiate-project-form"
            disabled={initiateMutation.isPending || !form.landParcelId || !form.name}
          >
            {initiateMutation.isPending ? 'Initiating…' : 'Initiate Project'}
          </button>
        </>
      }
    >
      <form id="initiate-project-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="form-field full">
            <label className="form-label">Land Parcel</label>
            <EntityCombobox
              value={form.landParcelId || null}
              onChange={id => setForm(f => ({ ...f, landParcelId: id ?? '', homeIds: [] }))}
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
            <label className="form-label">Project Name</label>
            <input
              className="form-input"
              type="text"
              value={form.name}
              onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
              placeholder="e.g. Unit 4A Renovation"
              required
            />
          </div>
          <div className="form-field full">
            <label className="form-label">Description (optional)</label>
            <textarea
              className="form-textarea"
              value={form.description}
              onChange={e => setForm(f => ({ ...f, description: e.target.value }))}
              placeholder="Describe the scope of work…"
            />
          </div>
          <div className="form-field">
            <label className="form-label">Projected Start (optional)</label>
            <input
              className="form-input"
              type="date"
              value={form.projectedStartDate}
              onChange={e => setForm(f => ({ ...f, projectedStartDate: e.target.value }))}
            />
          </div>
          <div className="form-field">
            <label className="form-label">Projected End (optional)</label>
            <input
              className="form-input"
              type="date"
              value={form.projectedEndDate}
              onChange={e => setForm(f => ({ ...f, projectedEndDate: e.target.value }))}
            />
          </div>
          <div className="form-field full">
            <label className="form-label">Budget (optional)</label>
            <input
              className="form-input"
              type="number"
              min="0"
              step="1"
              value={form.budget ?? ''}
              onChange={e => setForm(f => ({ ...f, budget: e.target.value ? parseFloat(e.target.value) : undefined }))}
              placeholder="0"
            />
          </div>
          {form.landParcelId && parcelHomes.length > 0 && (
            <div className="form-field full">
              <label className="form-label">Associated Homes (optional)</label>
              <div style={{ display: 'grid', gap: 6 }}>
                {parcelHomes.map(h => (
                  <label
                    key={h.id}
                    style={{ display: 'flex', gap: 8, alignItems: 'center', fontSize: 13, cursor: 'pointer' }}
                  >
                    <input
                      type="checkbox"
                      checked={(form.homeIds ?? []).includes(h.id!)}
                      onChange={() => toggleHomeId(h.id!)}
                    />
                    {HOME_TYPE_LABELS[h.buildingType ?? ''] ?? h.buildingType}
                    {' · '}
                    {h.bedrooms} bd / {Number(h.bathrooms).toFixed(1)} ba
                  </label>
                ))}
              </div>
            </div>
          )}
        </div>
        {initiateMutation.isError && (
          <div className="form-error">Failed to initiate project. Check the parcel and try again.</div>
        )}
      </form>
    </SlideOver>
  )
}
