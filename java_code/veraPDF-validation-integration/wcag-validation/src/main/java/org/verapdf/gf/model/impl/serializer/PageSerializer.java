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
import org.verapdf.gf.model.factory.chunks.CurveChunk;
import org.verapdf.wcag.algorithms.entities.IPage;
import org.verapdf.wcag.algorithms.entities.content.IChunk;
import org.verapdf.wcag.algorithms.entities.content.LineArtChunk;

import java.io.IOException;

/**
 * Serializes a semantic page and its directly exposed artifacts.
 *
 * <p>Every page carries its number and label and an {@code artifacts} array. Line-art containers and
 * individual curve chunks are excluded here because line-art is represented through semantic structure
 * content rather than this flat page-artifact list.</p>
 *
 * <p><b>.NET porting notes:</b> Implement as {@code JsonConverter<IPage>} using
 * {@code Utf8JsonWriter} in {@code VeraPdf.Validation.Wcag.Model.Impl.Serializer}. {@code IPage}, chunks,
 * and curve types from {@code org.verapdf.wcag.algorithms.*} have no .NET port yet. Keep the filtering rule
 * and array ordering consistent with the semantic tree serializer.</p>
 */
public class PageSerializer extends StdSerializer<IPage> {

	protected PageSerializer(Class<IPage> t) {
		super(t);
	}

	/** Writes page identity and supported non-curve artifacts. */
	@Override
	public void serialize(IPage page, JsonGenerator jsonGenerator, SerializerProvider serializerProvider)
			throws IOException {

		jsonGenerator.writeStartObject();
		jsonGenerator.writeNumberField("pageNumber", page.getPageNumber());
		jsonGenerator.writeStringField("pageLabel", page.getPageLabel());
		jsonGenerator.writeFieldName("artifacts");
		jsonGenerator.writeStartArray();
		for (IChunk chunk : page.getArtifacts()) {
			if (!(chunk instanceof LineArtChunk) && !(chunk instanceof CurveChunk)) {
				jsonGenerator.writeObject(chunk);
			}
		}
		jsonGenerator.writeEndArray();

		jsonGenerator.writeEndObject();
	}
}
