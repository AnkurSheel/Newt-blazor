#!/bin/bash

# Check if increment version is provided
INCREMENT_VERSION=$1

# Fetch the latest version tag (matching v0.0.0 or v0.0.0.0)
latest_tag=$(git describe --tags --long --match "v[0-9]*" 2>/dev/null || echo "")

if [ -z "$latest_tag" ]; then
  echo "No existing tags found. Defaulting base version to 0.0.0.1."
  version_base="0.0.0.1"
else
  # Strip leading 'v'
  latest_tag_clean=$(echo "$latest_tag" | sed 's/^v//')
  # Extract base version part before any git describe suffix like -6-g721dc1
  version_base=$(echo "$latest_tag_clean" | cut -d'-' -f1)
  echo "Found latest version: $version_base (from tag: $latest_tag)"
fi

# Split version into 4 parts (major.minor.patch.revision)
IFS='.' read -r major minor patch revision <<< "$version_base"
major=${major:-0}
minor=${minor:-0}
patch=${patch:-0}
revision=${revision:-0}

echo "Current parsed version: $major.$minor.$patch.$revision"

# Increment version based on input if provided
if [ -n "$INCREMENT_VERSION" ]; then
  case "$INCREMENT_VERSION" in
    revision|build)
      ((revision++))
      ;;
    patch)
      ((patch++))
      revision=0
      ;;
    minor)
      ((minor++))
      patch=0
      revision=0
      ;;
    major)
      ((major++))
      minor=0
      patch=0
      revision=0
      ;;
    "")
      ;;
    *)
      echo "Invalid increment_version value: $INCREMENT_VERSION. Expected: major, minor, patch, revision, or build"
      exit 1
      ;;
  esac

  new_version="${major}.${minor}.${patch}.${revision}"
  tag_version="v${new_version}"

  echo "New calculated version: $new_version ($tag_version)"

  # Automatically update CHANGELOG.md if present
  if [ -f "CHANGELOG.md" ]; then
    echo "Updating CHANGELOG.md for $new_version..."
    today=$(date +%Y-%m-%d)
    
    # Check if version is already recorded in CHANGELOG.md
    if ! grep -q "## \[$new_version\]" CHANGELOG.md && ! grep -q "## $new_version" CHANGELOG.md; then
      # Replace '## [Unreleased]' with '## [Unreleased]\n\n## [new_version] - YYYY-MM-DD'
      sed -i.bak -E "s/## *\[Unreleased\]/## [Unreleased]\n\n## [$new_version] - $today/" CHANGELOG.md
      rm -f CHANGELOG.md.bak
      echo "CHANGELOG.md updated successfully with version [$new_version] - $today."
    fi
  fi

  git config user.name "github-actions[bot]"
  git config user.email "github-actions[bot]@users.noreply.github.com"

  # Commit CHANGELOG.md changes if any exist
  if ! git diff --quiet CHANGELOG.md 2>/dev/null; then
    echo "Committing updated CHANGELOG.md..."
    git add CHANGELOG.md
    git commit -m "chore(release): update CHANGELOG for $tag_version [skip ci]"
    current_branch=$(git rev-parse --abbrev-ref HEAD)
    if [ "$current_branch" = "HEAD" ] && [ -n "$GITHUB_REF_NAME" ]; then
      current_branch="$GITHUB_REF_NAME"
    fi
    if [ -n "$current_branch" ] && [ "$current_branch" != "HEAD" ]; then
      git push origin "HEAD:$current_branch" || echo "Warning: could not push branch $current_branch to origin"
    fi
  fi

  if git rev-parse "$tag_version" >/dev/null 2>&1; then
    commit_hash=$(git rev-parse --short HEAD)
    tag_version="${tag_version}-${commit_hash}"
    echo "Tag $tag_version already exists. Using tag with commit hash."
  else
    echo "Tagging new version $tag_version."
    git tag "$tag_version"
    git push origin "$tag_version" || echo "Warning: could not push tag to origin"
  fi
else
  new_version="${major}.${minor}.${patch}.${revision}"
  tag_version="v${new_version}"
fi

echo "Final buildVersion: $new_version"
echo "Final tagVersion: $tag_version"

# Output for GitHub Actions
if [ -n "$GITHUB_OUTPUT" ]; then
  echo "buildVersion=$new_version" >> "$GITHUB_OUTPUT"
  echo "tagVersion=$tag_version" >> "$GITHUB_OUTPUT"
fi
