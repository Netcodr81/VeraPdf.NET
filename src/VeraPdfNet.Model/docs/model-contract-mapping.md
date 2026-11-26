# Verified Java-to-.NET contract mapping

## Authoritative evidence

The source declarations are the eight local `.mdl` files under
`java_code\veraPDF-model-integration\src\main\java`.
The matching upstream generation logic was inspected at the immutable
veraPDF/veraPDF-model-syntax revision
`f8e9f87d98a96784cd78a15229736722bb77eff8`:

- [ModelGenerator.xtend](https://github.com/veraPDF/veraPDF-model-syntax/blob/f8e9f87d98a96784cd78a15229736722bb77eff8/org.verapdf/src/org/verapdf/generator/ModelGenerator.xtend):
  `compile`, `generateGetter`, `toJavaType`, `getDependenceClass`,
  `appendDependenceClass` and `generateGenericModelObject`.
- [Model.xtext](https://github.com/veraPDF/veraPDF-model-syntax/blob/f8e9f87d98a96784cd78a15229736722bb77eff8/org.verapdf/src/org/verapdf/Model.xtext):
  declaration grammar and `%` documentation.

The emitter template itself establishes the generated signatures; this port
does not claim a Java build or Java adapter execution comparison. The initial
compatibility tests retain an independently recorded scalar/root baseline and
compare all source declarations with assembly reflection.

## Verified choices

| Upstream emission | Native C# contract |
| --- | --- |
| `Integer` -> boxed `Long` | `long?`; never `int` solely from the DSL name |
| `Decimal` -> boxed `Double` | `double?`; not CLR `decimal` |
| `Boolean` -> boxed `Boolean` | `bool?` |
| `String` -> `String` | `string?` |
| `List<String>` | `IReadOnlyList<string>?` |
| `List<List<String>>` | `IReadOnlyList<IReadOnlyList<string>>?` |
| `Set<String>` | `IReadOnlySet<string>?`; ordinal identifiers, no order promise |
| `get` + original property identifier | `Get` + original identifier; preserve case |
| `Object` interface | `VeraPdf.Model.BaseLayer.IModelObject` |
| Other interfaces / `extends` | `I` prefix / C# interface inheritance in layer namespaces |
| Link declaration | Metadata and named traversal, **no typed link getter** |
| `getLinkedObjects(String)` returning `List<? extends Object>` | `GetLinkedObjects(string)` returning `IReadOnlyList<IModelObject>` |
| Generated `ModelHelper` | `ModelHelper`, original simple-name indexing and derived-to-base declaration order |
| Generated `GenericModelObject` | Minimal native base with nullable context, false context-dependence default and unsupported-link exception |

Java reference-valued scalars/collections can be null. Nullability annotations
here intentionally allow missing/invalid states. Collection elements are modeled
as non-null strings/lists; no evidence in the DSL requires null elements, and
implementations must not introduce them as sentinels.

The nine root methods are `GetLinks`, `GetLinkedObjects`, `GetSuperTypes`,
`GetProperties`, `GetObjectType`, `GetID`, `IsContextDependent`,
`GetExtraContext`, and `GetContext`. IDs and context strings are producer-defined,
not invented by this contract library.

## Intentional .NET differences

Public collections are read-only views; helper collections are immutable
snapshots/frozen collections rather than mutable Java lists/maps.
Unknown helper type names and invalid generic-root construction fail explicitly
with argument errors. Unsupported link access uses `InvalidOperationException`
rather than Java `IllegalAccessError`. No parser failures are swallowed.

`ModelHelper.GetDefinition` additionally exposes source, CLR/Java names, scalar
types and link target/multiplicity, allowing full schema parity checks.
The embedded JSON manifest is generated data, not a runtime source-generation
dependency or a validation engine.

## Constraints for future adapters

Do not conflate absent dictionary entries, explicit PDF null and invalid types.
Named links may return empty traversal even when the schema says one: the
`PDSMaskImage.imageCS` documentation explicitly requires this.
Preserve array/operator order, original scalar spellings where relevant,
document-specific context and case-sensitive identifiers.
Do not substitute system fonts, normalized XML or rendered bitmaps for
validation evidence. The model contracts alone provide none of these algorithms.
