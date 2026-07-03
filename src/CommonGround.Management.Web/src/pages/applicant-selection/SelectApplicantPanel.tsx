import { useState } from 'react'
import { GroundLeaseTemplatesApi, HomesApi, LandParcelsApi } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import EntityCombobox from '../../components/EntityCombobox'
import SlideOver from '../../components/SlideOver'
import { useSelectApplicant } from '../../hooks/applicantSelection'
import { useToast } from '../../hooks/useToast'
import { apiConfig } from '../../lib/apiConfig'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

const landParcelsApi = new LandParcelsApi(apiConfig)
const homesApi = new HomesApi(apiConfig)
const groundLeaseTemplatesApi = new GroundLeaseTemplatesApi(apiConfig)

type FormState = {
  homeId: string
  landParcelId: string
  groundLeaseTemplateId: string
  purchasePrice: number
  selectedDate: string
}

const emptyForm = (): FormState => ({
  homeId: '',
  landParcelId: '',
  groundLeaseTemplateId: '',
  purchasePrice: 0,
  selectedDate: new Date().toISOString().slice(0, 10),
})

type Props = {
  applicationId: string
  onClose: () => void
}

export default function SelectApplicantPanel({ applicationId, onClose }: Props) {
  const [form, setForm] = useState<FormState>(emptyForm)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useSelectApplicant()
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)

    try {
      await mutation.mutateAsync({
        applicationId,
        request: {
          homeId: form.homeId,
          landParcelId: form.landParcelId,
          groundLeaseTemplateId: form.groundLeaseTemplateId,
          purchasePrice: Number(form.purchasePrice),
          selectedDate: form.selectedDate,
        },
      })
      success('Applicant selected')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to select applicant.'))
    }
  }

  return (
    <SlideOver
      title="Select Applicant"
      subtitle="Finalize applicant selection for a property"
      onClose={onClose}
      footer={
        <>
          <button className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button
            className="btn btn-primary"
            type="submit"
            form="select-applicant-form"
            disabled={
              mutation.isPending
              || !form.homeId
              || !form.landParcelId
              || !form.groundLeaseTemplateId
              || !form.purchasePrice
              || !form.selectedDate
            }
          >
            {mutation.isPending ? 'Selecting…' : 'Select Applicant'}
          </button>
        </>
      }
    >
      <form id="select-applicant-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="form-field full">
            <label className="form-label">Land Parcel</label>
            <EntityCombobox
              value={form.landParcelId || null}
              onChange={id => setForm(f => ({ ...f, landParcelId: id ?? '', homeId: '' }))}
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
              value={form.homeId || null}
              onChange={id => setForm(f => ({ ...f, homeId: id ?? '' }))}
              fetchOptions={async q => {
                const result = await homesApi.getHomes({ landParcelIds: [form.landParcelId], pageSize: 10 })
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
              placeholder={form.landParcelId ? 'Search homes on this parcel…' : 'Select a parcel first'}
              disabled={!form.landParcelId}
            />
          </div>
          <div className="form-field full">
            <label className="form-label">Ground Lease Template</label>
            <EntityCombobox
              value={form.groundLeaseTemplateId || null}
              onChange={id => setForm(f => ({ ...f, groundLeaseTemplateId: id ?? '' }))}
              fetchOptions={async q => {
                const result = await groundLeaseTemplatesApi.getGroundLeaseTemplates({
                  search: q,
                  status: 'Active',
                  pageSize: 10,
                })
                return (result.items ?? []).map(t => ({
                  id: t.id!,
                  label: t.templateName ?? t.id!,
                }))
              }}
              placeholder="Search templates…"
            />
          </div>
          <div className="form-field">
            <label className="form-label">Purchase Price ($)</label>
            <input
              className="form-input"
              type="number"
              min="1"
              step="1"
              value={form.purchasePrice}
              onChange={e => setForm(f => ({ ...f, purchasePrice: parseFloat(e.target.value) || 0 }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label">Selected Date</label>
            <input
              className="form-input"
              type="date"
              value={form.selectedDate}
              onChange={e => setForm(f => ({ ...f, selectedDate: e.target.value }))}
              required
            />
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
