#!/usr/bin/env bash
# Prints the feedback visitors sent with "Send feedback" on a demo server, newest first.
# Feedback is kept after a visitor's sample trust is deleted.
#
#   ./feedback.sh          the last 30 days
#   ./feedback.sh 7        the last 7 days
#   ./feedback.sh all      everything
set -euo pipefail

cd "$(dirname "$0")"
days="${1:-30}"
if [ "$days" = "all" ]; then
  since="'-infinity'::timestamptz"
elif [[ "$days" =~ ^[0-9]+$ ]]; then
  since="now() - interval '$days days'"
else
  echo "usage: $0 [days|all]" >&2
  exit 2
fi

docker compose exec -T postgres psql -U commonground -d commonground -X -q -P pager=off -P format=wrapped -c "
  select to_char((data->>'SentAt')::timestamptz, 'YYYY-MM-DD HH24:MI') as sent,
         coalesce(data->>'ReplyTo', '(no reply)') as reply_to,
         coalesce(data->>'Page', '') as page,
         data->>'Message' as message
  from demo.mt_doc_demofeedback
  where (data->>'SentAt')::timestamptz >= $since
  order by (data->>'SentAt')::timestamptz desc;"
