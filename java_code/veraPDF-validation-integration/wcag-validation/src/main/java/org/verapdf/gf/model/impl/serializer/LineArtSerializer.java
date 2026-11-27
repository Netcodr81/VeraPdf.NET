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
import org.verapdf.wcag.algorithms.entities.content.LineArtChunk;
import org.verapdf.wcag.algorithms.entities.content.LineChunk;

import java.io.IOException;

/**
 * Serializes a grouped vector region and its constituent stroked lines.
 *
 * <p>Each region has a page number and a left-bottom-right-top bounding box. The optional {@code lines}
 * array is omitted for an empty region; nonempty lines are delegated to {@link LineSerializer} so their
 * endpoints and stroke widths use the common line schema.</p>
 *
 * <p><b>.NET porting notes:</b> Implement as {@code JsonConverter<LineArtChunk>} with
 * {@code Utf8JsonWriter}; register {@code LineChunk} conversion alongside it. These WCAG algorithm chunk
 * types have no .NET port yet. Preserve omission of the empty {@code lines} field, not just an empty array.</p>
 */
public class LineArtSerializer extends StdSerializer<LineArtChunk> {

	protected LineArtSerializer(Class<LineArtChunk> t) {
		super(t);
	}

	/** Writes the vector-region bounds and, when present, its line children. */
	@Override
	public void serialize(LineArtChunk lineArtChunk, JsonGenerator jsonGenerator, SerializerProvider serializerProvider)
			throws IOException {

		jsonGenerator.writeStartObject();
		jsonGenerator.writeStringField("type", "LineArtChunk");
		jsonGenerator.writeNumberField("pageNumber", lineArtChunk.getPageNumber());
		jsonGenerator.writeArrayFieldStart("boundingBox");
		jsonGenerator.writeNumber(lineArtChunk.getLeftX());
		jsonGenerator.writeNumber(lineArtChunk.getBottomY());
		jsonGenerator.writeNumber(lineArtChunk.getRightX());
		jsonGenerator.writeNumber(lineArtChunk.getTopY());
		jsonGenerator.writeEndArray();
		if (!lineArtChunk.getLineChunks().isEmpty()) {
			jsonGenerator.writeArrayFieldStart("lines");
			for (LineChunk line : lineArtChunk.getLineChunks()) {
				jsonGenerator.writeObject(line);
			}
			jsonGenerator.writeEndArray();
		}
		jsonGenerator.writeEndObject();
	}
}
