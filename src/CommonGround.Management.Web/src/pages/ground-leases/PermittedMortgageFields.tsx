interface Props {
  lender: string
  noticeAddress: string
  onLenderChange: (value: string) => void
  onNoticeAddressChange: (value: string) => void
}

/** Lender and notice address, shared by Record Permitted Mortgage and Edit Permitted Mortgage. */
export function LenderFields({ lender, noticeAddress, onLenderChange, onNoticeAddressChange }: Props) {
  return (
    <>
      <div className="form-field full">
        <label className="form-label" htmlFor="mortgage-lender">Lender or Servicer</label>
        <input
          id="mortgage-lender"
          className="form-input"
          value={lender}
          onChange={e => onLenderChange(e.target.value)}
          required
        />
      </div>
      <div className="form-field full">
        <label className="form-label" htmlFor="mortgage-notice-address">Notice Address</label>
        <textarea
          id="mortgage-notice-address"
          className="form-textarea"
          value={noticeAddress}
          onChange={e => onNoticeAddressChange(e.target.value)}
          placeholder={'Loss Mitigation Dept.\n100 Main St\nSpringfield, IL 62701'}
          aria-describedby="mortgage-notice-address-hint"
        />
        <p id="mortgage-notice-address-hint" className="form-hint">
          Where the trust sends copies of notices of default. Optional; leave empty if not known yet.
        </p>
      </div>
    </>
  )
}
