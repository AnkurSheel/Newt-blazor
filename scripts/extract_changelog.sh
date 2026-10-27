#!/bin/bash

# scripts/extract_changelog.sh
# Usage: ./scripts/extract_changelog.sh <version_or_tag> <output_file>

VERSION_INPUT="$1"
OUTPUT_FILE="${2:-release_notes.md}"
CHANGELOG_FILE="CHANGELOG.md"

if [ ! -f "$CHANGELOG_FILE" ]; then
  echo "CHANGELOG.md not found. Creating empty release notes file."
  touch "$OUTPUT_FILE"
  exit 0
fi

# Clean version (strip leading 'v' and any git hash suffix)
CLEAN_VERSION=$(echo "$VERSION_INPUT" | sed 's/^v//' | cut -d'-' -f1)

echo "Extracting release notes for version '$VERSION_INPUT' (clean: '$CLEAN_VERSION') from $CHANGELOG_FILE..."

# Extract release notes section using pure awk
awk -v ver="$CLEAN_VERSION" '
  BEGIN { IGNORECASE=1; found=0 }
  /^## / {
    if (found) exit;
    if ($0 ~ "\\[" ver "\\]" || $0 ~ "## " ver) { found=1; next }
  }
  found { print }
' "$CHANGELOG_FILE" > "$OUTPUT_FILE"

# Fallback to [Unreleased] if specific version section is not found
if [ ! -s "$OUTPUT_FILE" ]; then
  awk '
    BEGIN { IGNORECASE=1; found=0 }
    /^## / {
      if (found) exit;
      if ($0 ~ "\\[unreleased\\]") { found=1; next }
    }
    found { print }
  ' "$CHANGELOG_FILE" > "$OUTPUT_FILE"
fi

if [ -s "$OUTPUT_FILE" ]; then
  echo "Extracted release notes successfully to $OUTPUT_FILE:"
  cat "$OUTPUT_FILE"
else
  echo "No specific changelog section found; release notes will fall back to automatic GitHub release notes."
fi
