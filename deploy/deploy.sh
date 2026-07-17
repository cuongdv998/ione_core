#!/usr/bin/env bash
#
# Deploy Docker Swarm stack (docker-stack.production.yml).
#
# Cách dùng:
#   ./deploy/deploy.sh <IMAGE_TAG>
#   IMAGE_TAG=abc1234 ./deploy/deploy.sh
#
# Biến tùy chọn:
#   STACK_NAME       Tên stack (mặc định: ione)
#   STACK_FILE         Đường dẫn file compose/stack (mặc định: cùng thư mục với script)
#   REGISTRY           Prefix ảnh: registry.gitlab.com/group/project (không có / cuối).
#                      docker login vào host đầu tiên (registry.gitlab.com) rồi pull .../ione-api:TAG
#   REGISTRY_USER      User đăng nhập registry (khi có REGISTRY)
#   REGISTRY_PASSWORD  Password/token (khi có REGISTRY)
#
# Các biến App / DB / MinIO… export trước khi gọi script sẽ được thay vào file stack (cùng cơ chế docker-compose).
#
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
STACK_FILE="${STACK_FILE:-${SCRIPT_DIR}/docker-stack.production.yml}"
STACK_NAME="${STACK_NAME:-ione}"

usage() {
  cat <<'EOF'
Usage: deploy/deploy.sh <IMAGE_TAG>
   hoặc: IMAGE_TAG=<sha> ./deploy/deploy.sh

Tùy chọn (env): STACK_NAME, STACK_FILE, REGISTRY, REGISTRY_USER, REGISTRY_PASSWORD
  — Nếu có REGISTRY: login, pull registry/.../ione-{api,angular}:TAG, tag thành ione-api:TAG trên máy rồi stack deploy.
EOF
  exit "${1:-0}"
}

if [[ "${1:-}" == "-h" || "${1:-}" == "--help" ]]; then
  usage 0
fi

IMAGE_TAG="${1:-${IMAGE_TAG:-}}"
if [[ -z "${IMAGE_TAG}" ]]; then
  echo "Thiếu IMAGE_TAG." >&2
  usage 1
fi

export IMAGE_TAG

if [[ ! -f "${STACK_FILE}" ]]; then
  echo "Không thấy file stack: ${STACK_FILE}" >&2
  exit 1
fi

if ! docker info --format '{{.Swarm.LocalNodeState}}' 2>/dev/null | grep -q active; then
  echo "Node này chưa tham gia Swarm (docker swarm init hoặc join)." >&2
  exit 1
fi

if [[ -n "${REGISTRY:-}" ]]; then
  : "${REGISTRY_USER:?Khi đặt REGISTRY cần REGISTRY_USER}"
  : "${REGISTRY_PASSWORD:?Khi đặt REGISTRY cần REGISTRY_PASSWORD}"
  LOGIN_HOST="${REGISTRY%%/*}"
  echo "docker login ${LOGIN_HOST}"
  printf '%s' "${REGISTRY_PASSWORD}" | docker login "${LOGIN_HOST}" -u "${REGISTRY_USER}" --password-stdin

  API_SRC="${REGISTRY}/ione-api:${IMAGE_TAG}"
  ANGULAR_SRC="${REGISTRY}/ione-angular:${IMAGE_TAG}"
  echo "Pull ${API_SRC}"
  docker pull "${API_SRC}"
  echo "Pull ${ANGULAR_SRC}"
  docker pull "${ANGULAR_SRC}"
  docker tag "${API_SRC}" "ione-api:${IMAGE_TAG}"
  docker tag "${ANGULAR_SRC}" "ione-angular:${IMAGE_TAG}"
fi

echo "Stack deploy: name=${STACK_NAME} file=${STACK_FILE} IMAGE_TAG=${IMAGE_TAG}"
docker stack deploy -c "${STACK_FILE}" "${STACK_NAME}" --with-registry-auth

echo "Xong. Kiểm tra: docker stack services ${STACK_NAME}"
