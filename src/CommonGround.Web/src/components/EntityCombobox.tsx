import { useEffect, useRef, useState } from 'react'
import { Check, ChevronsUpDown } from 'lucide-react'
import { cn } from '@/lib/utils'
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from '@/components/ui/command'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'

interface Option {
  id: string
  label: string
}

interface EntityComboboxProps {
  value: string | null
  onChange: (id: string | null) => void
  fetchOptions: (q: string) => Promise<Option[]>
  placeholder?: string
  disabled?: boolean
  id?: string
}

export default function EntityCombobox({
  value,
  onChange,
  fetchOptions,
  placeholder = 'Search…',
  disabled = false,
  id,
}: EntityComboboxProps) {
  const [open, setOpen] = useState(false)
  const [query, setQuery] = useState('')
  const [options, setOptions] = useState<Option[]>([])
  const [loading, setLoading] = useState(false)
  const [selectedLabel, setSelectedLabel] = useState<string | null>(null)

  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)
  const fetchSeqRef = useRef(0)
  const isMountedRef = useRef(true)

  useEffect(() => {
    isMountedRef.current = true
    return () => {
      isMountedRef.current = false
      if (debounceRef.current) clearTimeout(debounceRef.current)
    }
  }, [])

  // When value is cleared externally, reset display
  useEffect(() => {
    if (value === null) {
      setSelectedLabel(null)
    }
  }, [value])

  const runFetch = (q: string) => {
    const seq = ++fetchSeqRef.current
    setLoading(true)
    fetchOptions(q)
      .then(results => {
        if (!isMountedRef.current || seq !== fetchSeqRef.current) return
        setOptions(results)
        setLoading(false)
      })
      .catch(() => {
        if (!isMountedRef.current || seq !== fetchSeqRef.current) return
        setOptions([])
        setLoading(false)
      })
  }

  const handleOpenChange = (next: boolean) => {
    setOpen(next)
    if (next) {
      setQuery('')
      if (debounceRef.current) clearTimeout(debounceRef.current)
      // Fetch initial options immediately on open
      debounceRef.current = setTimeout(() => runFetch(''), 0)
    }
  }

  const handleQueryChange = (q: string) => {
    setQuery(q)
    if (debounceRef.current) clearTimeout(debounceRef.current)
    debounceRef.current = setTimeout(() => runFetch(q), 300)
  }

  const handleSelect = (opt: Option) => {
    if (value === opt.id) {
      // Clicking the selected item deselects it
      onChange(null)
      setSelectedLabel(null)
    } else {
      onChange(opt.id)
      setSelectedLabel(opt.label)
    }
    setOpen(false)
  }

  const displayLabel = selectedLabel ?? (value ? value : null)

  return (
    <Popover open={open} onOpenChange={handleOpenChange}>
      <PopoverTrigger asChild>
        <button
          id={id}
          type="button"
          role="combobox"
          aria-expanded={open}
          disabled={disabled}
          className={cn(
            'form-input flex items-center justify-between gap-2',
            displayLabel ? 'text-[var(--text-primary)]' : 'text-[var(--text-dim)]',
          )}
        >
          <span className="truncate">{displayLabel ?? placeholder}</span>
          <ChevronsUpDown className="size-4 shrink-0 opacity-50" />
        </button>
      </PopoverTrigger>
      <PopoverContent className="w-[var(--radix-popover-trigger-width)] p-0" align="start">
        <Command shouldFilter={false}>
          <CommandInput
            placeholder={placeholder}
            value={query}
            onValueChange={handleQueryChange}
          />
          <CommandList>
            {loading ? (
              <CommandEmpty>Loading…</CommandEmpty>
            ) : options.length === 0 ? (
              <CommandEmpty>No results found.</CommandEmpty>
            ) : (
              <CommandGroup>
                {options.map(opt => (
                  <CommandItem
                    key={opt.id}
                    value={opt.id}
                    onSelect={() => handleSelect(opt)}
                    data-checked={value === opt.id}
                  >
                    <Check
                      className={cn('mr-2 size-4', value === opt.id ? 'opacity-100' : 'opacity-0')}
                    />
                    {opt.label}
                  </CommandItem>
                ))}
              </CommandGroup>
            )}
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  )
}
