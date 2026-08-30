const StorageKey = 'uefi-mod-tools.ifr-viewer.ui-preferences.v1';

export type ThemeMode = 'dark' | 'light';

type UiPreferences = {
  themeMode: ThemeMode;
};

export function loadUiPreferences(): UiPreferences {
  try {
    const value: unknown = JSON.parse(localStorage.getItem(StorageKey) ?? '{}');
    const themeMode = typeof value === 'object' && value !== null
      ? (value as { themeMode?: unknown }).themeMode
      : undefined;
    if (themeMode === 'dark' || themeMode === 'light') {
      return { themeMode };
    }
  } catch {
    // Local storage can be unavailable for restrictive browser contexts.
  }

  return { themeMode: 'dark' };
}

export function saveUiPreferences(preferences: UiPreferences) {
  try {
    localStorage.setItem(StorageKey, JSON.stringify(preferences));
  } catch {
    // The viewer remains usable when browser storage is unavailable.
  }
}
