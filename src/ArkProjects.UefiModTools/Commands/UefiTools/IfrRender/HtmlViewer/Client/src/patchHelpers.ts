import type { Document, Node, SetupPatchQuestion } from './types';

export type PatchKind = 'setup' | 'sct';

export type SetupPatchChange = {
  label: string;
  original: number;
  patched: number;
};

type SetupPatchValue = {
  id: string;
  accessLevel: number | null;
  failsafe: number | null;
  optimal: number | null;
};

type SetupPatchDocument = {
  version: 1;
  type: 'AMI-SetupData-Patch';
  questions: SetupPatchValue[];
};

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

export function setupPatchChanges(node: Node, patch?: SetupPatchQuestion): SetupPatchChange[] {
  const source = node.SetupDataQuestion;
  if (!source || !patch) {
    return [];
  }

  return [
    ['Access level', source.AccessLevel, patch.question.accessLevel],
    ['Failsafe', source.Failsafe, patch.question.failsafe],
    ['Optimal', source.Optimal, patch.question.optimal],
  ].flatMap(([label, original, patched]) =>
    original === patched ? [] : [{ label, original, patched }] as SetupPatchChange[],
  );
}

export function createSetupPatchDocument(document: Document, patches: Record<number, SetupPatchQuestion>): SetupPatchDocument {
  const nodesByAddress = new Map(setupQuestionNodes(document).map(node => [node.SetupDataQuestion!.BeginAddress, node]));
  const questions = Object.values(patches).flatMap(patch => {
    const node = nodesByAddress.get(patch.beginAddress);
    const source = node?.SetupDataQuestion;
    if (!node || !source) {
      return [];
    }

    const value: SetupPatchValue = {
      id: setupPatchId(node),
      accessLevel: patch.question.accessLevel === source.AccessLevel ? null : patch.question.accessLevel,
      failsafe: patch.question.failsafe === source.Failsafe ? null : patch.question.failsafe,
      optimal: patch.question.optimal === source.Optimal ? null : patch.question.optimal,
    };
    return value.accessLevel === null && value.failsafe === null && value.optimal === null ? [] : [value];
  });

  return { version: 1, type: 'AMI-SetupData-Patch', questions };
}

export async function importPatch(file: File | undefined, kind: PatchKind, document?: Document) {
  if (!file) {
    return undefined;
  }

  const patch: unknown = JSON.parse(await file.text());

  if (kind === 'setup' && document && isSetupPatch(patch)) {
    const nodesById = new Map(setupQuestionNodes(document).map(node => [setupPatchId(node), node]));
    return {
      kind,
      setupPatches: Object.fromEntries(
        patch.questions.flatMap(question => {
          const node = nodesById.get(question.id);
          const source = node?.SetupDataQuestion;
          if (!node || !source) {
            return [];
          }

          const importedQuestion: SetupPatchQuestion = {
            beginAddress: source.BeginAddress,
            endAddress: source.EndAddress,
            type: node.Opcode,
            question: {
              questionId: source.QuestionId,
              pageId: source.PageId,
              accessLevel: question.accessLevel ?? source.AccessLevel,
              helpStringId: source.HelpStringId,
              promptStringId: source.PromptStringId,
              failsafe: question.failsafe ?? source.Failsafe,
              optimal: question.optimal ?? source.Optimal,
            },
          };
          return [[source.BeginAddress, importedQuestion]];
        }),
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

function isSetupPatch(value: unknown): value is SetupPatchDocument {
  return typeof value === 'object' && value !== null &&
    (value as { version?: unknown }).version === 1 &&
    (value as { type?: unknown }).type === 'AMI-SetupData-Patch' &&
    Array.isArray((value as { questions?: unknown }).questions);
}

function isSctPatch(value: unknown): value is { suppressIfPatches: { disable: boolean; offset: number }[] } {
  return typeof value === 'object' && value !== null && Array.isArray((value as { suppressIfPatches?: unknown }).suppressIfPatches);
}

function setupQuestionNodes(document: Document): Node[] {
  return document.Formsets.flatMap(formset => formset.Forms.flatMap(form => collectQuestionNodes(form.Children)));
}

function collectQuestionNodes(nodes: Node[]): Node[] {
  return nodes.flatMap(node => [node, ...collectQuestionNodes(node.Children)]).filter(node => node.SetupDataQuestion !== undefined);
}

function setupPatchId(node: Node): string {
  return `${node.Opcode}-${node.SetupDataQuestion!.QuestionId.toString(16).padStart(4, '0').toUpperCase()}`;
}
