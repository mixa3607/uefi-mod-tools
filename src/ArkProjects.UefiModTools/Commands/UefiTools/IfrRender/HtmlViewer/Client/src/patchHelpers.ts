import type { Node, SetupPatchQuestion } from './types';

export type PatchKind = 'setup' | 'sct';

export function downloadJson(name: string, value: unknown) {
  const blob = new Blob([JSON.stringify(value, null, 2)], { type: 'application/json' });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');

  link.href = url;
  link.download = name;
  link.click();

  setTimeout(() => URL.revokeObjectURL(url), 0);
}

export function setupPatch(node: Node, patches: Record<number, SetupPatchQuestion>) {
  const source = node.SetupDataQuestion!;

  return patches[source.BeginAddress] ?? {
    beginAddress: source.BeginAddress,
    endAddress: source.EndAddress,
    type: node.Opcode,
    question: {
      questionId: source.QuestionId,
      pageId: source.PageId,
      accessLevel: source.AccessLevel,
      helpStringId: source.HelpStringId,
      promptStringId: source.PromptStringId,
      failsafe: source.Failsafe,
      optimal: source.Optimal,
    },
  };
}

export async function importPatch(file: File | undefined, kind: PatchKind) {
  if (!file) {
    return undefined;
  }

  const patch: unknown = JSON.parse(await file.text());

  if (kind === 'setup' && isSetupPatch(patch)) {
    return {
      kind,
      setupPatches: Object.fromEntries(
        patch.questions.map(question => [question.beginAddress, question]),
      ),
    };
  }

  if (kind === 'sct' && isSctPatch(patch)) {
    return {
      kind,
      disabledSuppressions: patch.suppressIfPatches
        .filter(item => item.disable)
        .map(item => item.offset),
    };
  }

  throw new Error('Incompatible patch');
}

function isSetupPatch(value: unknown): value is { questions: SetupPatchQuestion[] } {
  return typeof value === 'object' && value !== null && Array.isArray((value as { questions?: unknown }).questions);
}

function isSctPatch(value: unknown): value is { suppressIfPatches: { disable: boolean; offset: number }[] } {
  return typeof value === 'object' && value !== null && Array.isArray((value as { suppressIfPatches?: unknown }).suppressIfPatches);
}
