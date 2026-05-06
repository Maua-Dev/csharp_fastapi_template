#!/usr/bin/env bash
set -euo pipefail

if [ -f ".env" ]; then
  set -a
  # shellcheck disable=SC1091
  source ".env"
  set +a
fi

TABLE_NAME="${DYNAMO_TABLE_NAME:-cs-fastapi-test-dynamo-table}"
ENDPOINT_URL="${DYNAMO_ENDPOINT_URL:-http://localhost:8000}"
AWS_REGION="${AWS_REGION:-us-east-1}"
AWS_ARGS=(
  --endpoint-url "$ENDPOINT_URL"
  --region "$AWS_REGION"
  --cli-connect-timeout 2
  --cli-read-timeout 5
  --no-cli-pager
)

export AWS_ACCESS_KEY_ID="${AWS_ACCESS_KEY_ID:-dummy}"
export AWS_SECRET_ACCESS_KEY="${AWS_SECRET_ACCESS_KEY:-dummy}"
export AWS_DEFAULT_REGION="${AWS_DEFAULT_REGION:-$AWS_REGION}"

if ! aws dynamodb describe-table --table-name "$TABLE_NAME" "${AWS_ARGS[@]}" >/dev/null 2>&1; then
  aws dynamodb create-table \
    --table-name "$TABLE_NAME" \
    --attribute-definitions AttributeName=item_id,AttributeType=S \
    --key-schema AttributeName=item_id,KeyType=HASH \
    --billing-mode PAY_PER_REQUEST \
    "${AWS_ARGS[@]}" \
    >/dev/null

  aws dynamodb wait table-exists --table-name "$TABLE_NAME" "${AWS_ARGS[@]}"
fi

request_file="$(mktemp)"
trap 'rm -f "$request_file"' EXIT

cat > "$request_file" <<JSON
{
  "$TABLE_NAME": [
    {
      "DeleteRequest": {
        "Key": {
          "item_id": { "S": "2f8ea77a-839c-4f14-8eb3-90f140f9d3e1" }
        }
      }
    },
    {
      "PutRequest": {
        "Item": {
          "item_id": { "S": "b11af449-22c7-43db-b0e4-dbfbbe7fdbd7" },
          "name": { "S": "Barbie" },
          "price": { "N": "48.9" },
          "item_type": { "S": "Toy" },
          "admin_permission": { "BOOL": false }
        }
      }
    },
    {
      "PutRequest": {
        "Item": {
          "item_id": { "S": "b21af449-22c7-43db-b0e4-dbfbbe7fdbd7" },
          "name": { "S": "Hamburguer" },
          "price": { "N": "38" },
          "item_type": { "S": "Food" },
          "admin_permission": { "BOOL": false }
        }
      }
    },
    {
      "PutRequest": {
        "Item": {
          "item_id": { "S": "b31af449-22c7-43db-b0e4-dbfbbe7fdbd7" },
          "name": { "S": "T-shirt" },
          "price": { "N": "22.95" },
          "item_type": { "S": "Clothes" },
          "admin_permission": { "BOOL": false }
        }
      }
    },
    {
      "PutRequest": {
        "Item": {
          "item_id": { "S": "b41af449-22c7-43db-b0e4-dbfbbe7fdbd7" },
          "name": { "S": "Super Mario Bros" },
          "price": { "N": "55" },
          "item_type": { "S": "Games" },
          "admin_permission": { "BOOL": true }
        }
      }
    }
  ]
}
JSON

aws dynamodb batch-write-item \
  --request-items "file://$request_file" \
  "${AWS_ARGS[@]}" \
  >/dev/null

echo "Loaded seed items into $TABLE_NAME at $ENDPOINT_URL"
