import type { Document, Expression, Node, NodeRef, Option, SelectableNode } from './types';
import type { ChipProps } from '@mui/material';

export type DocumentIndex = {
  byId: Map<string, NodeRef>;
  questionsById: Map<number, QuestionNodeRef[]>;
};

export type QuestionNodeRef = NodeRef & { node: Node };

export type OptionDefault = {
  value: number;
  text: string;
};

export function label(value?: { text: string }) {
  return value?.text ?? '';
}

export function nodeId(node: SelectableNode) {
  return `${node.NodeType}:${node.Source.Offset}`;
}

export function optionValue(option: Option) {
  return typeof option.Value === 'number' ? option.Value : option.Value?.value;
}

export function expressionText(expression: Expression) {
  switch (expression.Opcode) {
    case 'True':
      return 'always true';
    case 'False':
      return 'always false';
    case 'EqIdVal':
      return `Question #${expression.QuestionId} equals ${String(expression.Value)}`;
    case 'EqIdId':
      return `Question #${expression.QuestionId} equals Question #${expression.OtherQuestionId}`;
    default:
      return expression.Opcode;
  }
}

export function conditionText(node: Node) {
  const expressions = node.ExpressionOperations.map(expressionText).join(' -> ');
  return `${(node.Effect ?? node.Opcode).toUpperCase()} when ${expressions || 'expression unavailable'}`;
}

export function nodeColor(node: Node): ChipProps['color'] {
  if (node.NodeType === 'question') {
    return 'success';
  }

  switch (node.Effect) {
    case 'suppress':
      return 'error';
    case 'disable':
      return 'warning';
    case 'grayout':
      return 'info';
    default:
      return 'secondary';
  }
}

export function indexDocument(document: Document): DocumentIndex {
  const byId = new Map<string, NodeRef>();
  const questionsById = new Map<number, QuestionNodeRef[]>();

  document.Formsets.forEach(formset => {
    const formsetReference: NodeRef = {
      id: nodeId(formset),
      node: formset,
      parentIds: [],
      formTitle: label(formset.Title) || formset.Guid || 'Formset',
    };
    byId.set(formsetReference.id, formsetReference);

    formset.Forms.forEach(form => {
      const formReference: NodeRef = {
        id: nodeId(form),
        node: form,
        parentIds: [formsetReference.id],
        formTitle: label(form.Title) || `Form ${form.Id ?? '?'}`,
      };
      byId.set(formReference.id, formReference);

      const visit = (node: Node, parentIds: string[]) => {
        const reference: QuestionNodeRef = {
          id: nodeId(node),
          node,
          parentIds,
          formTitle: label(form.Title) || `Form ${form.Id ?? '?'}`,
        };

        byId.set(reference.id, reference);

        if (node.QuestionId != null) {
          questionsById.set(node.QuestionId, [
            ...(questionsById.get(node.QuestionId) ?? []),
            reference,
          ]);
        }

        node.Children.forEach(child => visit(child, [...parentIds, reference.id]));
      };

      form.Children.forEach(node => visit(node, [...formReference.parentIds, formReference.id]));
    });
  });

  return { byId, questionsById };
}

export function questionDefaults(node: Node): OptionDefault[] {
  return node.Options
    .map(option => ({ value: optionValue(option), text: label(option.Text) }))
    .filter(
      (option): option is OptionDefault =>
        typeof option.value === 'number' &&
        Number.isInteger(option.value) &&
        option.value >= 0 &&
        option.value <= 255,
    );
}
