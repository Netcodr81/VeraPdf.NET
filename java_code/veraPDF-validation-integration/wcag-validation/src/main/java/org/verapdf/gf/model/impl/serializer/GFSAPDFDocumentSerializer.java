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

import com.fasterxml.jackson.core.Version;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.module.SimpleModule;
import org.verapdf.gf.model.impl.sa.GFSAAnnotationNode;
import org.verapdf.gf.model.impl.sa.GFSAPDFDocument;
import org.verapdf.gf.model.impl.sa.GFSAStructElem;
import org.verapdf.wcag.algorithms.entities.IPage;
import org.verapdf.wcag.algorithms.entities.content.ImageChunk;
import org.verapdf.wcag.algorithms.entities.content.LineArtChunk;
import org.verapdf.wcag.algorithms.entities.content.LineChunk;
import org.verapdf.wcag.algorithms.entities.content.TextChunk;

import java.io.File;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 * Configures the JSON serializers for semantic PDF and WCAG content objects and writes a document to a file.
 *
 * <p>A single Jackson {@link ObjectMapper} is configured at class initialization with serializers for the
 * document, structure nodes, pages, text, images, annotations, lines, and line-art regions. The shared mapper
 * is only configured once and then reused. Serialization failures are logged as warnings rather than
 * propagated to the caller, which is an important part of the current API behavior.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested static facade:
 * {@code VeraPdf.Validation.Wcag.Model.Impl.Serializer.GFSAPDFDocumentSerializer}. Register explicit
 * {@code System.Text.Json.JsonConverter<T>} implementations in one {@code JsonSerializerOptions} instance
 * and write files with {@code JsonSerializer.Serialize}. The semantic entity library
 * {@code org.verapdf.wcag.algorithms.*} has no .NET port yet. Keep options immutable after first use, and
 * decide deliberately whether to preserve the current log-and-swallow failure policy or expose exceptions.</p>
 */
public class GFSAPDFDocumentSerializer {

	private static final Logger LOGGER = Logger.getLogger(GFSAPDFDocumentSerializer.class.getCanonicalName());

	private static final ObjectMapper objectMapper = new ObjectMapper();

	static {

		SimpleModule module = new SimpleModule("NodeSerializer", new Version(2, 1,
				3, null, null, null));

		DocumentSerializer documentSerializer = new DocumentSerializer(GFSAPDFDocument.class);
		module.addSerializer(GFSAPDFDocument.class, documentSerializer);

		NodeSerializer nodeSerializer = new NodeSerializer(GFSAStructElem.class);
		module.addSerializer(GFSAStructElem.class, nodeSerializer);

		PageSerializer pageSerializer = new PageSerializer(IPage.class);
		module.addSerializer(IPage.class, pageSerializer);

		TextSerializer textSerializer = new TextSerializer(TextChunk.class);
		module.addSerializer(TextChunk.class, textSerializer);

		ImageSerializer imageSerializer = new ImageSerializer(ImageChunk.class);
		module.addSerializer(ImageChunk.class, imageSerializer);

		AnnotationNodeSerializer annotationSerializer = new AnnotationNodeSerializer(GFSAAnnotationNode.class);
		module.addSerializer(GFSAAnnotationNode.class, annotationSerializer);

		LineArtSerializer lineArtSerializer = new LineArtSerializer(LineArtChunk.class);
		module.addSerializer(LineArtChunk.class, lineArtSerializer);

		LineSerializer lineSerializer = new LineSerializer(LineChunk.class);
		module.addSerializer(LineChunk.class, lineSerializer);
		objectMapper.registerModule(module);
	}

	/** Serializes one semantic document to the requested file; failures are logged and suppressed. */
	public static void serialize(File file, GFSAPDFDocument document) {
		try {
			objectMapper.writeValue(file, document);
		} catch (Exception e) {
			LOGGER.log(Level.WARNING, "Exception during serializing GFSAPDFDocument: " + e.getMessage());
		}
	}
}
