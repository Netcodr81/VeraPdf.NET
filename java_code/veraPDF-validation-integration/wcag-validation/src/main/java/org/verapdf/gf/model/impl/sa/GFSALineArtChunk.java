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

import org.verapdf.model.salayer.SALineArtChunk;
import org.verapdf.wcag.algorithms.entities.content.LineArtChunk;

/**
 * Exposes an algorithm line-art chunk and its page-location context to validation rules.
 *
 * <p>Line-art chunks capture vector drawing content that may convey meaning without being text
 * or a raster image. Their context comes from the algorithm's computed bounding-box location.</p>
 *
 * <p><b>.NET porting notes:</b> Implement as
 * {@code VeraPdf.Validation.Wcag.Model.Impl.Sa.GFSALineArtChunk} implementing
 * {@code VeraPdf.Model.SALayer.ISALineArtChunk}. The algorithm {@code LineArtChunk} and its
 * bounding-box semantics need a .NET equivalent; do not substitute a raw PDF rectangle.</p>
 *
 * @author Maxim Plushchov
 */
public class GFSALineArtChunk extends GFSAChunk implements SALineArtChunk {

	/** Model type name used when rules select line-art chunks. */
	public static final String LINE_ART_CHUNK_TYPE = "SALineArtChunk";

	public final LineArtChunk lineArtChunk;

	public GFSALineArtChunk(LineArtChunk lineArtChunk) {
		super(LINE_ART_CHUNK_TYPE);
		this.lineArtChunk = lineArtChunk;
	}

	@Override
	/** Returns the normalized location string used as the rule context. */
	public String getContext() {
		return lineArtChunk.getBoundingBox().getLocation();
	}

}
