/*
 * This file is part of veraPDF WCAG Validation, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF WCAG Validation is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF WCAG Validation as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF WCAG Validation as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.gf.model.impl.serializer;

import com.fasterxml.jackson.core.JsonGenerator;
import com.fasterxml.jackson.databind.SerializerProvider;
import com.fasterxml.jackson.databind.ser.std.StdSerializer;
import org.verapdf.gf.model.impl.sa.*;
import org.verapdf.wcag.algorithms.entities.IAttributesDictionary;
import org.verapdf.wcag.algorithms.entities.INode;
import org.verapdf.wcag.algorithms.entities.SemanticAnnot;

import java.io.IOException;
import java.util.Objects;

/**
 * Serializes a tagged-PDF structure element and its semantic children.
 *
 * <p>The structure element's standard role becomes the JSON {@code type}, with missing roles labeled
 * {@code NonStandard}; its COS object number provides source identity. Table cells include row and column
 * spans from their attributes. Children may be semantic wrappers around text, image, and line-art chunks,
 * so the serializer unwraps those wrappers and includes semantic annotation nodes from the algorithm node.</p>
 *
 * <p><b>.NET porting notes:</b> Implement as {@code JsonConverter<GFSAStructElem>} with
 * {@code Utf8JsonWriter} and explicit pattern matching for the wrapper types. The structure/semantic types
 * from {@code org.verapdf.wcag.algorithms.*} have no .NET port yet. Preserve child ordering and unwrap only
 * the same wrapper classes; annotations currently serialize the first annotation associated with a node.</p>
 */
public class NodeSerializer extends StdSerializer<GFSAStructElem> {

	protected NodeSerializer(Class<GFSAStructElem> t) {
		super(t);
	}

	/** Writes one structure element, its optional table attributes, and its children. */
	@Override
	public void serialize(GFSAStructElem elem, JsonGenerator jsonGenerator, SerializerProvider serializerProvider)
			throws IOException {

		jsonGenerator.writeStartObject();
		String type = elem.getstandardType();
		if (type == null) {
			type = "NonStandard";
		}
		jsonGenerator.writeStringField("type", type);
		jsonGenerator.writeNumberField("objectKeyNumber", elem.getStructElemDictionary().getObject().getKey().getNumber());
		if (Objects.equals(elem.getstandardType(), "TD") || Objects.equals(elem.getstandardType(), "TH")) {
			IAttributesDictionary AttributesDictionary = elem.getNode().getAttributesDictionary();
			jsonGenerator.writeFieldName("attributes");
			jsonGenerator.writeStartObject();
			jsonGenerator.writeNumberField("colSpan", AttributesDictionary.getColSpan());
			jsonGenerator.writeNumberField("rowSpan", AttributesDictionary.getRowSpan());
			jsonGenerator.writeEndObject();
		}
		if (!elem.getChildren().isEmpty() || !elem.getNode().getChildren().isEmpty()) {
			jsonGenerator.writeFieldName("children");
			jsonGenerator.writeStartArray();
			for (Object child : elem.getChildren()) {
				if (child instanceof GFSATextChunk) {
					jsonGenerator.writeObject(((GFSATextChunk) child).getTextChunk());
				} else if (child instanceof GFSAImageChunk) {
					jsonGenerator.writeObject(((GFSAImageChunk) child).imageChunk);
				} else if (child instanceof GFSALineArtChunk) {
					jsonGenerator.writeObject(((GFSALineArtChunk) child).lineArtChunk);
				} else {
					jsonGenerator.writeObject(child);
				}
			}
			for (INode child : elem.getNode().getChildren()) {
				if (child instanceof SemanticAnnot) {
					jsonGenerator.writeObject(((SemanticAnnot)child).getAnnots().get(0));
				}
			}
			jsonGenerator.writeEndArray();
		}

		jsonGenerator.writeEndObject();
	}
}
