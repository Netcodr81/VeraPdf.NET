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
package org.verapdf.gf.model.factory.chunks;

import org.verapdf.cos.COSBase;
import org.verapdf.cos.COSKey;
import org.verapdf.gf.model.impl.containers.StaticStorages;
import org.verapdf.pd.PDResourcesHandler;
import org.verapdf.operator.Operator;
import org.verapdf.wcag.algorithms.entities.content.IChunk;

import java.util.*;

/**
 * Converts a parsed content stream's alternating operands and operators into WCAG content chunks.
 *
 * <p>The parser receives tokens from veraPDF's PDF content-stream interpreter. Operands accumulate until
 * an operator consumes them; a {@link ChunkParser} then tracks text and graphics state, and finalizes
 * line art after all tokens have been visited. The result is a page's extracted visual artifacts, while
 * chunks inside marked content are also indexed in {@link StaticStorages} for structure-tree lookup.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested static utility in {@code VeraPdf.Validation.Wcag.Model.Factory.Chunks}.
 * Map {@code List<Object>} to an enumerable/token sequence and {@code List<COSBase>} to a typed list, using
 * C# {@code is} patterns for operand/operator dispatch. {@code org.verapdf.wcag.algorithms.*} and its
 * {@code IChunk} types have no .NET port yet, so chunk model types must be ported with the parser.</p>
 *
 * @author Maxim Plushchov
 */
public final class ChunkFactory {

	/**
	 * Extract chunks from one stream, preserving inherited form state and source identity.
	 *
	 * @param pageNumber one-based page number associated with emitted geometry
	 * @param objectKey COS object key of the stream, or null for a direct page stream
	 * @param rawTokens parsed operands and operators in their original order
	 * @param inheritedGraphicState graphics state inherited by a nested form
	 * @param resourcesHandler resources used to resolve fonts, colors, and XObjects
	 * @param parentObjectKey parent stream identity for untagged nested-form content
	 * @param markedContent parent MCID inherited by nested content, if any
	 * @param xObjectName resource name when parsing a Form XObject
	 * @return artifacts emitted directly by this stream; tagged chunks are stored separately
	 */
	public static List<IChunk> chunksFromTokens(Integer pageNumber, COSKey objectKey, List<Object> rawTokens,
										 GraphicsState inheritedGraphicState, PDResourcesHandler resourcesHandler,
										 COSKey parentObjectKey, Long markedContent, String xObjectName) {
		List<COSBase> arguments = new ArrayList<>();
		ChunkParser parser = new ChunkParser(pageNumber, objectKey, inheritedGraphicState, resourcesHandler,
				parentObjectKey, markedContent, xObjectName);
		for (int operatorIndex = 0; operatorIndex < rawTokens.size(); operatorIndex++) {
			Object rawToken = rawTokens.get(operatorIndex);
			if (rawToken instanceof COSBase) {
				arguments.add((COSBase) rawToken);
			} else if (rawToken instanceof Operator) {
				parser.parseChunk(((Operator) rawToken), arguments, operatorIndex);
				arguments = new ArrayList<>();
			}
		}
		if (StaticStorages.getIsIgnoreMCIDs()) {
			parser.processLineArts();
		} else {
			parser.parseLineArts();
		}
		return parser.getArtifacts();
	}
}
