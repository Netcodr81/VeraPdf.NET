/*
 * This file is part of veraPDF Validation, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF Validation is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF Validation as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF Validation as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.gf.model.impl.pd.annotations;

import org.verapdf.gf.model.impl.pd.GFPDAnnot;
import org.verapdf.pd.PDResourcesHandler;
import org.verapdf.model.pdlayer.PDMovieAnnot;
import org.verapdf.pd.PDAnnotation;
import org.verapdf.pd.PDPage;

/**
 * Represents a movie annotation that references a PDF movie object.
 *
 * <p>Movie annotations describe time-based media and may trigger viewer playback.
 * Such interactive media is restricted in PDF/A, so this adapter primarily provides
 * the correct subtype for profile dispatch while inheriting common annotation fields.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested type:
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Annotations.GFPDMovieAnnot}, implementing
 * {@code VeraPdf.Model.PDLayer.IPDMovieAnnot}; reuse the common {@code GFPDAnnot} mapping
 * and do not add playback behavior.</p>
 *
 * @author Maxim Plushchov
 */
public class GFPDMovieAnnot extends GFPDAnnot implements PDMovieAnnot {

	/** Model type name used for movie-annotation profile rules. */
	public static final String MOVIE_ANNOTATION_TYPE = "PDMovieAnnot";

	public GFPDMovieAnnot(PDAnnotation annot, PDResourcesHandler pageResources, PDPage page) {
		super(annot, pageResources, page, MOVIE_ANNOTATION_TYPE);
	}

}
