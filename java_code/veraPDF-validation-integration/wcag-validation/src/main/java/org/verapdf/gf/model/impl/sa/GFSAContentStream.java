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
package org.verapdf.gf.model.impl.sa;

import org.verapdf.as.io.ASInputStream;
import org.verapdf.cos.COSKey;
import org.verapdf.cos.COSObjType;
import org.verapdf.cos.COSObject;
import org.verapdf.cos.COSStream;
import org.verapdf.gf.model.factory.chunks.ChunkFactory;
import org.verapdf.gf.model.factory.chunks.GraphicsState;
import org.verapdf.pd.PDResourcesHandler;
import org.verapdf.parser.PDFStreamParser;
import org.verapdf.wcag.algorithms.entities.content.IChunk;

import java.io.IOException;
import java.util.Collections;
import java.util.List;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 * Lazily parses one page or form content stream into WCAG algorithm chunks.
 *
 * <p>PDF content streams encode drawing and text operators, not semantic text records. The chunk
 * factory interprets those tokens using the inherited graphics state and resource handler and
 * returns geometric text, image, and line-art objects. Streams without a page context and
 * undecodable or unsupported contents produce an empty list; IO failures are logged and also
 * degrade to an empty list.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested adapter is
 * {@code VeraPdf.Validation.Wcag.Model.Impl.Sa.GFSAContentStream}. Use
 * {@code VeraPdf.Parser.Io.PdfSeekableStream} and {@code IDisposable} for decoded stream
 * lifetimes, and {@code ILogger<T>} instead of {@code java.util.logging}. The parser/chunk
 * algorithm libraries are not yet ported; preserve the one-time lazy parse and empty-result
 * fallback.</p>
 *
 * @author Maxim Plushchov
 */
public class GFSAContentStream {

	private static final Logger LOGGER = Logger.getLogger(GFSAContentStream.class.getName());

	private final Integer pageNumber;
	private final COSKey objectKey;
	private List<IChunk> artifacts = null;
	private final PDResourcesHandler resourcesHandler;
	private final GraphicsState inheritedGraphicsState;
	private final org.verapdf.pd.PDContentStream contentStream;
	private final Long markedContent;
	private final COSKey parentObjectKey;
	private final String xObjectName;

	public GFSAContentStream(org.verapdf.pd.PDContentStream contentStream, GraphicsState inheritedGraphicsState,
							 PDResourcesHandler resourcesHandler, Integer pageNumber, COSKey objectKey,
							 COSKey parentObjectKey, Long markedContent, String xObjectName) {
		this.pageNumber = pageNumber;
		this.objectKey = objectKey;
		this.contentStream = contentStream;
		this.resourcesHandler = resourcesHandler;
		this.inheritedGraphicsState = inheritedGraphicsState;
		this.markedContent = markedContent;
		this.parentObjectKey = parentObjectKey;
		this.xObjectName = xObjectName;
	}

	/** Parses once on first access and returns the resulting visual content chunks. */
	public List<IChunk> getArtifacts() {
		if (this.artifacts == null) {
			if (pageNumber != null) {
				parseChunks();
			} else {
				this.artifacts = Collections.emptyList();
			}
		}
		return this.artifacts;
	}

	/** Decodes tokens and delegates their interpretation to the WCAG chunk factory. */
	protected void parseChunks() {
		if (this.contentStream != null) {
			try {
				COSObject contentStream = this.contentStream.getContents();
				if (contentStream.getType() == COSObjType.COS_STREAM || contentStream.getType() == COSObjType.COS_ARRAY) {
					try (ASInputStream opStream = contentStream.getDirectBase().getData(COSStream.FilterFlags.DECODE)) {
						try (PDFStreamParser streamParser = new PDFStreamParser(opStream)) {
							streamParser.parseTokens();
							this.artifacts = ChunkFactory.chunksFromTokens(pageNumber, objectKey,
									streamParser.getTokens(), inheritedGraphicsState, resourcesHandler,
									parentObjectKey, markedContent, xObjectName);
						}
					}
				} else {
                    this.artifacts = Collections.emptyList();
                }
			} catch (IOException e) {
				LOGGER.log(Level.WARNING, "Error while parsing content stream. " + e.getMessage());
				this.artifacts = Collections.emptyList();
			}
		}
	}

}
