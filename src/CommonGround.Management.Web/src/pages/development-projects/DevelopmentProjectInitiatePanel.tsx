import { useState } from 'react'
import { Check, ChevronsUpDown, X } from 'lucide-react'
import { cn } from '@/lib/utils'
import { useInitiateDevelopmentProject } from '../../hooks/developmentProjects'
import type { InitiateDevelopmentProjectRequest } from '../../api'
import type { HomeListItemResponse } from '../../api'
import EntityCombobox from '../../components/EntityCombobox'
import SlideOver from '../../components/SlideOver'
import { LandParcelsApi } from '../../api'
import { useHomes } from '../../hooks/homes'
import { apiConfig } from '../../lib/apiConfig'
import { useToast } from '../../hooks/useToast'
import { HOME_TYPE_LABELS } from '../../hooks/types'
import { getErrorMessage } from '../../lib/getErrorMessage'
import { Badge } from '@/components/ui/badge'
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from '@/components/ui/command'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'

const landParcelsApi = new LandParcelsApi(apiConfig)

const parcelLabel = (p: { legalParcelNumber?: string | null; address?: string | null }) =>
  [p.legalParcelNumber, p.address].filter(Boolean).join(' — ')

const fetchParcelLabel = async (landParcelId: string) =>
  parcelLabel(await landParcelsApi.getLandParcel({ landParcelId }))

const emptyForm = (
  parcelId = '',
  homeIds: string[] = [],
): InitiateDevelopmentProjectRequest => ({
  landParcelId: parcelId,
  name: '',
  type: homeIds.length > 0 ? 'Rehabilitation' : 'NewConstruction',
  description: '',
  existingHomes: homeIds,
})

function homeLabel(h: HomeListItemResponse): string {
  const parts: string[] = []
  if (h.homeType) parts.push(HOME_TYPE_LABELS[h.homeType] ?? h.homeType)
  const details: string[] = []
  if (h.bedrooms != null) details.push(`${h.bedrooms}bd`)
  if (h.bathrooms != null) details.push(`${h.bathrooms}ba`)
  if (details.length > 0) parts.push(details.join(' / '))
  return parts.join(' — ')
}

interface HomesMultiSelectProps {
  homes: HomeListItemResponse[]
  selected: string[]
  onToggle: (id: string) => void
  disabled?: boolean
}

function HomesMultiSelect({ homes, selected, onToggle, disabled }: HomesMultiSelectProps) {
  const [open, setOpen] = useState(false)
  const [search, setSearch] = useState('')

  const filtered = search
    ? homes.filter(h => homeLabel(h).toLowerCase().includes(search.toLowerCase()))
    : homes

  const selectedHomes = homes.filter(h => selected.includes(h.id!))

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <button
          type="button"
          role="combobox"
          aria-expanded={open}
          disabled={disabled}
          className="form-input flex min-h-[38px] flex-wrap items-center justify-between gap-2"
        >
          {selectedHomes.length === 0 ? (
            <span className="text-[var(--text-dim)]">Select homes…</span>
          ) : (
            <div className="flex flex-wrap gap-1">
              {selectedHomes.map(h => (
                <Badge
                  key={h.id}
                  variant="secondary"
                  className="gap-1 border border-[var(--border)] bg-[var(--surface)] pr-1 text-[var(--text-primary)]"
                  onClick={e => {
                    if (disabled) return
                    e.stopPropagation()
                    onToggle(h.id!)
                  }}
                >
                  {homeLabel(h)}
                  <X className="size-3 cursor-pointer" />
                </Badge>
              ))}
            </div>
          )}
          <ChevronsUpDown className="ml-2 size-4 shrink-0 opacity-50" />
        </button>
      </PopoverTrigger>
      <PopoverContent className="w-[var(--radix-popover-trigger-width)] p-0" align="start">
        <Command shouldFilter={false}>
          <CommandInput
            placeholder="Filter homes…"
            value={search}
            onValueChange={setSearch}
          />
          <CommandList>
            {filtered.length === 0 ? (
              <CommandEmpty>No homes found.</CommandEmpty>
            ) : (
              <CommandGroup>
                {filtered.map(h => {
                  const isSelected = selected.includes(h.id!)
                  return (
                    <CommandItem
                      key={h.id}
                      value={h.id!}
                      onSelect={() => {
                        if (disabled) return
                        onToggle(h.id!)
                      }}
                      data-checked={isSelected}
                    >
                      <Check
                        className={cn('mr-2 size-4', isSelected ? 'opacity-100' : 'opacity-0')}
                      />
                      {homeLabel(h)}
                    </CommandItem>
                  )
                })}
              </CommandGroup>
            )}
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  )
}

type Props = {
  onClose: () => void
  initialParcelId?: string
  initialHomeIds?: never
} | {
  onClose: () => void
  initialParcelId: string
  initialHomeIds?: string[]
}

export default function DevelopmentProjectInitiatePanel({
  onClose,
  initialParcelId,
  initialHomeIds,
}: Props) {
  const [form, setForm] = useState<InitiateDevelopmentProjectRequest>(() =>
    emptyForm(initialParcelId, initialHomeIds ?? []),
  )
  const [initiateError, setInitiateError] = useState<string | null>(null)
  const initiateMutation = useInitiateDevelopmentProject()
  const { success } = useToast()

  const parcelSelectionLocked = !!initialParcelId
  const homesSelectionLocked = initialHomeIds !== undefined
  const isRehab = form.type === 'Rehabilitation'

  const { data: parcelHomes, isLoading: homesLoading } = useHomes({
    pageNumber: 1,
    pageSize: 100,
    landParcelId: form.landParcelId,
    enabled: isRehab && !!form.landParcelId,
  })
  const availableHomes = parcelHomes?.items ?? []

  const toggleHome = (id: string) => {
    setForm(f => {
      const current = f.existingHomes ?? []
      return {
        ...f,
        existingHomes: current.includes(id) ? current.filter(h => h !== id) : [...current, id],
      }
    })
  }

  const canSubmit =
    !!form.landParcelId &&
    !!form.name?.trim() &&
    !!form.description?.trim() &&
    (!isRehab || (form.existingHomes?.length ?? 0) > 0) &&
    !initiateMutation.isPending

  const handleSubmit = async (e: { preventDefault(): void }) => {
    e.preventDefault()
    setInitiateError(null)
    try {
      await initiateMutation.mutateAsync({
        landParcelId: form.landParcelId,
        name: form.name,
        type: form.type,
        description: form.description,
        existingHomes: isRehab ? form.existingHomes : undefined,
      })
      success('Development project initiated')
      onClose()
    } catch (error) {
      setInitiateError(await getErrorMessage(error, 'Failed to initiate project. Check the details and try again.'))
    }
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
            disabled={!canSubmit}
          >
            {initiateMutation.isPending ? 'Initiating…' : 'Initiate Project'}
          </button>
        </>
      }
    >
      <form id="initiate-project-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="form-field full">
            <label className="form-label">Project Type</label>
            <div className="flex gap-2">
              {(['NewConstruction', 'Rehabilitation'] as const).map(t => (
                <button
                  key={t}
                  type="button"
                  className={`btn ${form.type === t ? 'btn-primary' : 'btn-ghost'}`}
                  onClick={() =>
                    setForm(f => ({
                      ...f,
                      type: t,
                      existingHomes: homesSelectionLocked ? f.existingHomes : [],
                    }))
                  }
                >
                  {t === 'NewConstruction' ? 'New Construction' : 'Rehabilitation'}
                </button>
              ))}
            </div>
          </div>

          <div className="form-field full">
            <label className="form-label">Land Parcel</label>
            <EntityCombobox
              value={form.landParcelId || null}
              onChange={id => setForm(f => ({ ...f, landParcelId: id ?? '', existingHomes: [] }))}
              disabled={parcelSelectionLocked}
              fetchOptions={async q => {
                const result = await landParcelsApi.getLandParcels({ search: q, pageSize: 10 })
                return (result.items ?? []).map(p => ({ id: p.id!, label: parcelLabel(p) }))
              }}
              fetchLabel={fetchParcelLabel}
              placeholder="Search parcels…"
            />
          </div>

          <div className="form-field full">
            <label className="form-label">Name</label>
            <input
              className="form-input"
              value={form.name ?? ''}
              onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
              placeholder="Project name…"
            />
          </div>

          {isRehab && (
            <div className="form-field full">
              <label className="form-label">Homes</label>
              {!form.landParcelId ? (
                <p className="form-hint">Select a parcel to load homes.</p>
              ) : homesLoading ? (
                <p className="form-hint">Loading homes…</p>
              ) : availableHomes.length === 0 ? (
                <p className="form-hint">No homes found on this parcel.</p>
              ) : (
                <HomesMultiSelect
                  homes={availableHomes}
                  selected={form.existingHomes ?? []}
                  onToggle={toggleHome}
                  disabled={homesSelectionLocked}
                />
              )}
            </div>
          )}

          <div className="form-field full">
            <label className="form-label">Description</label>
            <textarea
              className="form-textarea"
              value={form.description ?? ''}
              onChange={e => setForm(f => ({ ...f, description: e.target.value }))}
              placeholder="Describe the scope of work…"
            />
          </div>
        </div>

        {initiateError && (
          <div className="form-error">{initiateError}</div>
        )}
      </form>
    </SlideOver>
  )
}
