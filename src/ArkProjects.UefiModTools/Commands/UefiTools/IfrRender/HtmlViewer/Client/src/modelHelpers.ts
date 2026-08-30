import type { Document, Expression, Node, NodeRef, Option } from './types';

export type DocumentIndex = {
  byId: Map<string, NodeRef>;
  questionsById: Map<number, NodeRef[]>;
};

export type OptionDefault = {
  value: number;
  text: string;
};

export function label(value?: { text: string }) {
  return value?.text ?? '';
}

export function nodeId(node: Node) {
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

export function indexDocument(document: Document): DocumentIndex {
  const byId = new Map<string, NodeRef>();
  const questionsById = new Map<number, NodeRef[]>();

  document.Formsets.forEach((formset, formsetIndex) => {
    formset.Forms.forEach((form, formIndex) => {
      const formId = `form:${formsetIndex}:${formIndex}`;

      const visit = (node: Node, parentIds: string[]) => {
        const reference: NodeRef = {
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

      form.Children.forEach(node => visit(node, [`formset:${formsetIndex}`, formId]));
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
