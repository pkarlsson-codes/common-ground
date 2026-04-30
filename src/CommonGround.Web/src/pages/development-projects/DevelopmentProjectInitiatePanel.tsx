import { useState } from 'react'
import { useInitiateDevelopmentProject } from '../../hooks/developmentProjects'
import type { InitiateDevelopmentProjectRequest } from '../../api'
import EntityCombobox from '../../components/EntityCombobox'
import SlideOver from '../../components/SlideOver'
import { LandParcelsApi } from '../../api'
import { apiConfig } from '../../lib/apiConfig'
import { useToast } from '../../hooks/useToast'

const landParcelsApi = new LandParcelsApi(apiConfig)

const emptyForm = (parcelId = ''): InitiateDevelopmentProjectRequest => ({
  landParcelId: parcelId,
  description: '',
})

interface Props {
  onClose: () => void
  initialParcelId?: string
}

export default function DevelopmentProjectInitiatePanel({ onClose, initialParcelId }: Props) {
  const [form, setForm] = useState<InitiateDevelopmentProjectRequest>(() => emptyForm(initialParcelId))
  const initiateMutation = useInitiateDevelopmentProject()
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await initiateMutation.mutateAsync({
        landParcelId: form.landParcelId,
        description: form.description || undefined,
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
            disabled={initiateMutation.isPending || !form.landParcelId}
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
            <label className="form-label">Description (optional)</label>
            <textarea
              className="form-textarea"
              value={form.description ?? ''}
              onChange={e => setForm(f => ({ ...f, description: e.target.value }))}
              placeholder="Describe the scope of work…"
            />
          </div>
        </div>
        {initiateMutation.isError && (
          <div className="form-error">Failed to initiate project. Check the parcel and try again.</div>
        )}
      </form>
    </SlideOver>
  )
}
