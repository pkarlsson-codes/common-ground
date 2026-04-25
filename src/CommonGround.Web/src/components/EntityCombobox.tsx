import { useEffect, useRef, useState } from 'react'

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

type DropdownState =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'error' }
  | { status: 'ready'; options: Option[] }

export default function EntityCombobox({
  value,
  onChange,
  fetchOptions,
  placeholder = 'Search…',
  disabled = false,
  id,
}: EntityComboboxProps) {
  const [inputText, setInputText] = useState('')
  const [open, setOpen] = useState(false)
  const [dropdown, setDropdown] = useState<DropdownState>({ status: 'idle' })
  const [activeIndex, setActiveIndex] = useState(-1)
  const [selectedLabel, setSelectedLabel] = useState<string | null>(null)

  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)
  const containerRef = useRef<HTMLDivElement>(null)
  const listRef = useRef<HTMLUListElement>(null)

  // When value is cleared externally, reset display
  useEffect(() => {
    if (value === null) {
      setSelectedLabel(null)
      setInputText('')
    }
  }, [value])

  const runFetch = (q: string) => {
    setDropdown({ status: 'loading' })
    setActiveIndex(-1)
    fetchOptions(q)
      .then(options => setDropdown({ status: 'ready', options }))
      .catch(() => setDropdown({ status: 'error' }))
  }

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const text = e.target.value
    setInputText(text)
    setOpen(true)

    // If user cleared the text, propagate null
    if (text === '') {
      onChange(null)
      setSelectedLabel(null)
    }

    if (debounceRef.current) clearTimeout(debounceRef.current)
    debounceRef.current = setTimeout(() => runFetch(text), 300)
  }

  const handleFocus = () => {
    setOpen(true)
    if (debounceRef.current) clearTimeout(debounceRef.current)
    debounceRef.current = setTimeout(() => runFetch(inputText), 300)
  }

  const selectOption = (option: Option) => {
    setSelectedLabel(option.label)
    setInputText(option.label)
    onChange(option.id)
    setOpen(false)
    setActiveIndex(-1)
  }

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (!open) return
    const options = dropdown.status === 'ready' ? dropdown.options : []

    if (e.key === 'ArrowDown') {
      e.preventDefault()
      setActiveIndex(i => Math.min(i + 1, options.length - 1))
    } else if (e.key === 'ArrowUp') {
      e.preventDefault()
      setActiveIndex(i => Math.max(i - 1, 0))
    } else if (e.key === 'Enter') {
      e.preventDefault()
      if (activeIndex >= 0 && activeIndex < options.length) {
        selectOption(options[activeIndex])
      }
    } else if (e.key === 'Escape') {
      setOpen(false)
      // Restore display to last confirmed selection
      setInputText(selectedLabel ?? '')
    }
  }

  // Close on outside click
  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setOpen(false)
        setInputText(selectedLabel ?? '')
      }
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [selectedLabel])

  // Scroll active item into view
  useEffect(() => {
    if (activeIndex >= 0 && listRef.current) {
      const item = listRef.current.children[activeIndex] as HTMLElement | undefined
      item?.scrollIntoView({ block: 'nearest' })
    }
  }, [activeIndex])

  const renderDropdownItems = () => {
    if (dropdown.status === 'loading') {
      return <li className="entity-combobox__option entity-combobox__option--disabled">Loading…</li>
    }
    if (dropdown.status === 'error') {
      return <li className="entity-combobox__option entity-combobox__option--disabled">Failed to load</li>
    }
    if (dropdown.status === 'ready') {
      if (dropdown.options.length === 0) {
        return <li className="entity-combobox__option entity-combobox__option--disabled">No results</li>
      }
      return dropdown.options.map((opt, i) => (
        <li
          key={opt.id}
          className={
            'entity-combobox__option' +
            (i === activeIndex ? ' entity-combobox__option--active' : '')
          }
          onMouseDown={e => {
            e.preventDefault() // prevent blur before click registers
            selectOption(opt)
          }}
          onMouseEnter={() => setActiveIndex(i)}
        >
          {opt.label}
        </li>
      ))
    }
    return null
  }

  return (
    <div className="entity-combobox" ref={containerRef}>
      <input
        id={id}
        className="form-input entity-combobox__input"
        value={inputText}
        onChange={handleInputChange}
        onFocus={handleFocus}
        onKeyDown={handleKeyDown}
        placeholder={placeholder}
        disabled={disabled}
        autoComplete="off"
        role="combobox"
        aria-expanded={open}
        aria-autocomplete="list"
      />
      {open && (
        <ul className="entity-combobox__dropdown" ref={listRef} role="listbox">
          {renderDropdownItems()}
        </ul>
      )}
    </div>
  )
}
